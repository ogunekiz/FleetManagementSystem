pipeline {
    agent any

    environment {
        DOTNET_CLI_HOME = '/tmp/dotnet'
        DOTNET_INSTALL_DIR = '/var/jenkins_home/dotnet'
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = '1'
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        // Konsoldaki NETSDK1188 dil/locale uyarılarını (warning) gizlemek için eklenen flags:
        MSBUILDDISABLENODEREUSE = '1'
        MSBUILDENABLEALLPROPERTYFUNCTIONS = '1'
        PATH = "$PATH:/var/jenkins_home/dotnet:/root/.dotnet/tools"
        WIN_SERVER_IP = '192.168.1.8' // Windows Server IIS IP
    }

    stages {
        stage('0. Setup .NET 9 SDK') {
            steps {
                echo '⚙️ .NET 9 SDK ortamı ve bağımlılıklar hazırlanıyor...'
                sh '''
                    mkdir -p $DOTNET_INSTALL_DIR
                    if [ ! -f "$DOTNET_INSTALL_DIR/dotnet" ]; then
                        echo "📥 .NET 9 SDK indiriliyor ve kuruluyor..."
                        curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --version 9.0.100 --install-dir $DOTNET_INSTALL_DIR
                    else
                        echo "✅ .NET SDK zaten yüklü."
                    fi
                    $DOTNET_INSTALL_DIR/dotnet --version
                '''
            }
        }

        stage('1. Checkout Code') {
            steps {
                echo '📥 GitHub repolarından kodlar çekiliyor...'
                git branch: 'main', url: 'https://github.com/ogunekiz/FleetManagementSystem.git'
            }
        }

        stage('2. Restore & Build') {
            steps {
                echo '🔨 .NET 9 Projesi derleniyor...'
                sh '''
                    dotnet restore FleetManagementSystem.sln
                    dotnet build FleetManagementSystem.sln --configuration Release --no-restore -clp:NoSummary -warnaserror:NETSDK1188=false /p:WarningLevel=1
                '''
            }
        }

        stage('3. Run Unit & Integration Tests') {
            steps {
                echo '🧪 Unit ve Integration testler koşturuluyor...'
                sh '''
                    dotnet test FleetManagementSystem.sln --configuration Release --no-build --logger "junit;LogFilePath=test_results.xml" -clp:NoSummary || true
                '''
            }
            post {
                always {
                    catchError(buildResult: 'SUCCESS', stageResult: 'UNSTABLE') {
                        // mstest yerine yüklü olan JUnit plugin kullanıldı
                        junit testResults: '**/*.xml', allowEmptyResults: true
                    }
                }
            }
        }

        stage('4. SAST - SonarQube Code Security Scan') {
            steps {
                echo '🛡️ SonarQube SAST ve OWASP Top 10 güvenlik taraması başlatılıyor...'
                catchError(buildResult: 'SUCCESS', stageResult: 'UNSTABLE') {
                    withCredentials([string(credentialsId: 'sonarqube-token', variable: 'SONAR_TOKEN')]) {
                        withSonarQubeEnv('SonarQube') {
                            sh '''
                                export PATH="$PATH:$HOME/.dotnet/tools"
                                dotnet tool install --global dotnet-sonarscanner || true
                                
                                # dotnet-sonarscanner doğrudan 'dotnet sonarscanner' olarak çağrılıyor
                                dotnet sonarscanner begin /k:"FleetManagementSystem" \
                                  /d:sonar.host.url="http://devsecops_sonarqube:9000" \
                                  /d:sonar.token="$SONAR_TOKEN"

                                dotnet build FleetManagementSystem.sln --configuration Release -clp:NoSummary

                                dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
                            '''
                        }
                    }
                }
            }
        }

        stage('5. Quality Gate Evaluation') {
            steps {
                timeout(time: 5, unit: 'MINUTES') {
                    script {
                        echo '⏳ SonarQube Quality Gate onay kontrolü...'
                        catchError(buildResult: 'SUCCESS', stageResult: 'UNSTABLE') {
                            waitForQualityGate abortPipeline: false
                        }
                    }
                }
            }
        }

        stage('6. Deploy to Production (IIS)') {
            steps {
                echo '🚀 Canlı Windows Server IIS ortamına publish ediliyor...'
                withCredentials([usernamePassword(credentialsId: 'win-server-creds', passwordVariable: 'WIN_PASS', usernameVariable: 'WIN_USER')]) {
                    sh '''
                        # 1. Linux Agent üzerinde artifact üret
                        dotnet publish FleetManagement.WebApi/FleetManagement.WebApi.csproj -c Release -o ./publish -clp:NoSummary

                        # 2. Gerekli sshpass paketini kontrol et/yükle
                        command -v sshpass >/dev/null 2>&1 || apt-get update && apt-get install -y sshpass

                        # 3. IIS App Pool'u durdur (Kilitli DLL hatası almamak için)
                        sshpass -p "$WIN_PASS" ssh -o StrictHostKeyChecking=no ${WIN_USER}@${WIN_SERVER_IP} "powershell -Command Stop-WebAppPool -Name FleetManagementApi" || true

                        # 4. Publish dosyalarını SCP ile Windows Server'a aktar
                        sshpass -p "$WIN_PASS" scp -r -o StrictHostKeyChecking=no ./publish/* ${WIN_USER}@${WIN_SERVER_IP}:C:/inetpub/wwwroot/FleetApi/

                        # 5. IIS App Pool'u tekrar başlat
                        sshpass -p "$WIN_PASS" ssh -o StrictHostKeyChecking=no ${WIN_USER}@${WIN_SERVER_IP} "powershell -Command Start-WebAppPool -Name FleetManagementApi"
                    '''
                }
            }
        }

        stage('7. DAST - OWASP ZAP Security Scan') {
            steps {
                echo '🔍 Canlı API üzerinde OWASP ZAP ile Dinamik Güvenlik Taraması (DAST) yapılıyor...'
                catchError(buildResult: 'SUCCESS', stageResult: 'UNSTABLE') {
                    sh '''
                        docker run --rm -v $(pwd):/zap/wrk/:rw -t ghcr.io/zaproxy/zaproxy:stable zap-api-scan.py \
                        -t http://${WIN_SERVER_IP}:6161/swagger/v1/swagger.json -f openapi -r zap_report.html || true
                    '''
                }
            }
            post {
                always {
                    catchError(buildResult: 'SUCCESS', stageResult: 'UNSTABLE') {
                        publishHTML([
                            allowMissing: true,
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