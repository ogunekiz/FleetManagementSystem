pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = "/tmp/dotnet"
        PATH = "$PATH:/root/.dotnet/tools"
        SONAR_SCANNER_HOME = tool 'SonarQubeScanner'
        WIN_SERVER_IP = '192.168.1.8' // Windows Server IIS IP
    }

    stages {
        stage('1. Checkout Code') {
            steps {
                echo '📥 GitHub repolarından kodlar çekiliyor...'
                git branch: 'main', url: 'https://github.com/ogunekiz/FleetManagementSystem.git'
            }
        }

        stage('2. Restore & Build') {
            steps {
                echo '🔨 .NET 9 Projesi derleniyor...'
                sh 'dotnet restore FleetManagementSystem.sln'
                sh 'dotnet build FleetManagementSystem.sln --configuration Release --no-restore'
            }
        }

        stage('3. Run Unit & Integration Tests') {
            steps {
                echo '🧪 Unit ve Integration testler koşturuluyor...'
                sh 'dotnet test FleetManagementSystem.sln --configuration Release --no-build --logger "trx;LogFileName=test_results.trx"'
            }
            post {
                always {
                    mstest testResultsFile: '**/*.trx'
                }
            }
        }

        stage('4. SAST - SonarQube Code Security Scan') {
            steps {
                echo '🛡️ SonarQube SAST ve OWASP Top 10 güvenlik taraması başlatılıyor...'
                withSonarQubeEnv('SonarQube') {
                    sh '''
                        dotnet tool install --global dotnet-sonarscanner || true
                        dotnet-sonarscanner begin /k:"FleetManagementSystem" /d:sonar.host.url="http://devsecops_sonarqube:9000" /d:sonar.token="$SONAR_AUTH_TOKEN"
                        dotnet build FleetManagementSystem.sln --configuration Release
                        dotnet-sonarscanner end /d:sonar.token="$SONAR_AUTH_TOKEN"
                    '''
                }
            }
        }

        stage('5. Quality Gate Evaluation') {
            steps {
                timeout(time: 5, unit: 'MINUTES') {
                    script {
                        echo '⏳ SonarQube Quality Gate onay kontrolü...'
                        // Quality gate sonucu başarısız olursa pipeline otomatik durdurulur
                        waitForQualityGate abortPipeline: true
                    }
                }
            }
        }

        stage('6. Deploy to Production (IIS)') {
            steps {
                echo '🚀 Canlı Windows Server IIS ortamına publish ediliyor...'
                sh 'dotnet publish FleetManagement.WebApi/FleetManagement.WebApi.csproj -c Release -o ./publish'
                
                // WebDeploy veya PowerShell/WinRM vasıtasıyla IIS sunucusuna aktarım
                sh "powershell -Command 'Copy-Item -Path ./publish/* -Destination \\\\${WIN_SERVER_IP}\\c$\\inetpub\\wwwroot\\FleetApi -Recurse -Force'"
            }
        }

        stage('7. DAST - OWASP ZAP Security Scan') {
            steps {
                echo '🔍 Canlı API üzerinde OWASP ZAP ile Dinamik Güvenlik Taraması (DAST) yapılıyor...'
                sh '''
                    docker run --rm -v $(pwd):/zap/wrk/:rw -t ghcr.io/zaproxy/zaproxy:stable zap-api-scan.py \
                    -t http://${WIN_SERVER_IP}/swagger/v1/swagger.json -f openapi -r zap_report.html || true
                '''
            }
            post {
                always {
                    publishHTML([allowMissing: true, alwaysLinkToLastBuild: true, keepAll: true, reportDir: '.', reportFiles: 'zap_report.html', reportName: 'OWASP ZAP DAST Report'])
                }
            }
        }
    }

    post {
        success {
            echo '✅ DevSecOps Pipeline başarıyla tamamlandı! Kod IIS üzerinde canlıda.'
        }
        failure {
            echo '❌ Pipeline sırasında hata alındı veya güvenlik testlerinden geçilemedi!'
        }
    }
}