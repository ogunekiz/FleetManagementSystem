pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = '/tmp/dotnet'
        DOTNET_INSTALL_DIR = '/var/jenkins_home/dotnet'
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = '1'
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        PATH = "$PATH:/var/jenkins_home/dotnet:/root/.dotnet/tools:/var/jenkins_home/.dotnet/tools"
        WIN_SERVER_IP = '192.168.1.8'
    }

    stages {
        stage('0. Setup .NET 9 SDK') {
            steps {
                echo '⚙️ .NET 9 SDK ortamı hazırlanıyor...'
                sh '''
                    mkdir -p $DOTNET_INSTALL_DIR
                    if [ ! -f "$DOTNET_INSTALL_DIR/dotnet" ]; then
                        curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --version 9.0.100 --install-dir $DOTNET_INSTALL_DIR
                    fi
                    $DOTNET_INSTALL_DIR/dotnet --version
                '''
            }
        }

        stage('1. Checkout Code') {
            steps {
                echo '📥 Kodlar çekiliyor...'
                git branch: 'main', url: 'https://github.com/ogunekiz/FleetManagementSystem.git'
            }
        }

        stage('2. Restore & Build') {
            steps {
                echo '🔨 .NET 9 Projesi derleniyor...'
                sh '''
                    dotnet restore FleetManagementSystem.sln
                    dotnet build FleetManagementSystem.sln --configuration Release --no-restore -p:NoWarn=NETSDK1188 -clp:NoSummary
                '''
            }
        }

        stage('3. Run Unit & Integration Tests') {
            steps {
                echo '🧪 Testler koşturuluyor...'
                sh '''
                    dotnet test FleetManagementSystem.sln --configuration Release --no-build -p:NoWarn=NETSDK1188 -clp:NoSummary --logger "junit;LogFilePath=test_results.xml"
                '''
            }
            post {
                always {
                    junit testResults: '**/test_results.xml', allowEmptyResults: false
                }
            }
        }

        stage('4. SAST - SonarQube Code Security Scan') {
            steps {
                echo '🛡️ SonarQube SAST taraması...'
                withCredentials([string(credentialsId: 'sonarqube-token', variable: 'SONAR_TOKEN')]) {
                    withSonarQubeEnv('SonarQube') {
                        sh '''
                            dotnet tool install --global dotnet-sonarscanner || true
                            
                            dotnet sonarscanner begin /k:"FleetManagementSystem" \
                              /d:sonar.host.url="http://devsecops_sonarqube:9000" \
                              /d:sonar.token="$SONAR_TOKEN"

                            dotnet build FleetManagementSystem.sln --configuration Release -p:NoWarn=NETSDK1188 -clp:NoSummary

                            dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
                        '''
                    }
                }
            }
        }

        stage('5. Quality Gate Evaluation') {
            steps {
                timeout(time: 5, unit: 'MINUTES') {
                    script {
                        echo '⏳ SonarQube Quality Gate onay kontrolü...'
                        waitForQualityGate abortPipeline: true
                    }
                }
            }
        }

        stage('6. Deploy to Production (IIS)') {
            steps {
                echo '🚀 IIS sunucusuna publish ediliyor...'
                withCredentials([usernamePassword(credentialsId: 'win-server-creds', passwordVariable: 'WIN_PASS', usernameVariable: 'WIN_USER')]) {
                    sh '''
                        dotnet publish FleetManagement.WebApi/FleetManagement.WebApi.csproj -c Release -o ./publish -p:NoWarn=NETSDK1188 -clp:NoSummary

                        command -v sshpass >/dev/null 2>&1 || apt-get update && apt-get install -y sshpass

                        # WebSite ve AppPool durdurma
                        sshpass -p "$WIN_PASS" ssh -o StrictHostKeyChecking=no ${WIN_USER}@${WIN_SERVER_IP} "powershell -Command Stop-Website -Name 'FleetManagementApi' -ErrorAction SilentlyContinue; Stop-WebAppPool -Name 'FleetManagementApi' -ErrorAction SilentlyContinue" || true

                        # Dosya Kopyalama
                        sshpass -p "$WIN_PASS" scp -r -o StrictHostKeyChecking=no ./publish/* ${WIN_USER}@${WIN_SERVER_IP}:C:/inetpub/wwwroot/FleetApi/

                        # IIS servislerini tekrar başlat
                        sshpass -p "$WIN_PASS" ssh -o StrictHostKeyChecking=no ${WIN_USER}@${WIN_SERVER_IP} "powershell -Command Start-WebAppPool -Name 'FleetManagementApi'; Start-Website -Name 'FleetManagementApi'"
                    '''
                }
            }
        }

        stage('7. DAST - OWASP ZAP Security Scan') {
            steps {
                echo '🔍 OWASP ZAP DAST taraması...'
                sh '''
                    docker run --rm -v $(pwd):/zap/wrk/:rw -t ghcr.io/zaproxy/zaproxy:stable zap-api-scan.py \
                    -t http://${WIN_SERVER_IP}:6161/swagger/v1/swagger.json -f openapi -r zap_report.html
                '''
            }
            post {
                always {
                    publishHTML([
                        allowMissing: false,
                        alwaysLinkToLastBuild: true,
                        keepAll: true,
                        reportDir: '.',
                        reportFiles: 'zap_report.html',
                        reportName: 'OWASP ZAP DAST Report'
                    ])
                }
            }
        }
    }

    post {
        success {
            echo '✅ DevSecOps Pipeline başarıyla tamamlandı!'
        }
        failure {
            echo '❌ Pipeline sırasında hata alındı!'
        }
    }
}