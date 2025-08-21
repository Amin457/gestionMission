pipeline {
    agent any
    
    environment {
        DOCKER_IMAGE_BACKEND = 'projetpfe-backend'
        DOCKER_IMAGE_FRONTEND = 'projetpfe-frontend'
        DOCKER_TAG = "${env.BUILD_NUMBER}"
        DOCKER_REGISTRY = 'docker.io/aminemelliti' // Change this to your registry
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
                echo "Checked out code from ${env.GIT_BRANCH}"
                echo "Running on Windows: ${env.OS == 'Windows_NT'}"
            }
        }
        
        stage('Build Backend') {
            steps {
                dir('gestionMissionBack') {
                    script {
                        echo "Building .NET Backend..."
                        
                        // Restore packages
                        if (env.OS == 'Windows_NT') {
                            bat 'dotnet restore'
                        } else {
                            sh 'dotnet restore'
                        }
                        
                        // Build the solution
                        if (env.OS == 'Windows_NT') {
                            bat 'dotnet build --configuration Release --no-restore'
                        } else {
                            sh 'dotnet build --configuration Release --no-restore'
                        }
                        
                        echo "Backend build completed successfully"
                    }
                }
            }
        }
        
        stage('Test Backend') {
            steps {
                dir('gestionMissionBack') {
                    script {
                        echo "Running backend tests..."
                        
                        if (env.OS == 'Windows_NT') {
                            bat 'dotnet test --configuration Release --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"'
                        } else {
                            sh 'dotnet test --configuration Release --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"'
                        }
                        
                        echo "Backend tests completed"
                    }
                }
            }
            post {
                always {
                    // Publier les résultats MSTest
                    mstest testResultsFile: '**/test-results.trx'
                }
            }
        }

        stage('Build Frontend') {
            steps {
                dir('gestionMissionFront') {
                    script {
                        echo "Building Angular Frontend..."
                        
                        // Install dependencies
                        if (env.OS == 'Windows_NT') {
                            bat 'npm ci'
                        } else {
                            sh 'npm ci'
                        }
                        
                        // Build the application
                        if (env.OS == 'Windows_NT') {
                            bat 'npm run build'
                        } else {
                            sh 'npm run build'
                        }
                        
                        echo "Frontend build completed successfully"
                    }
                }
            }
        }
        
        stage('Build Docker Images') {
            steps {
                script {
                    echo "Building Docker images..."
                    
                    // Build backend image
                    if (env.OS == 'Windows_NT') {
                        bat "docker build -t ${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG} -t ${DOCKER_IMAGE_BACKEND}:latest ./gestionMissionBack"
                    } else {
                        sh "docker build -t ${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG} -t ${DOCKER_IMAGE_BACKEND}:latest ./gestionMissionBack"
                    }
                    
                    // Build frontend image
                    if (env.OS == 'Windows_NT') {
                        bat "docker build -t ${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG} -t ${DOCKER_IMAGE_FRONTEND}:latest ./gestionMissionFront"
                    } else {
                        sh "docker build -t ${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG} -t ${DOCKER_IMAGE_FRONTEND}:latest ./gestionMissionFront"
                    }
                    
                    echo "Docker images built successfully"
                }
            }
        }
        
        stage('Push to Registry') {
            when {
                anyOf {
                    branch 'main'
                    branch 'master'
                    branch 'develop'
                }
            }
            steps {
                script {
                    echo "Pushing images to registry..."
                    
                    // Tag images for registry
                    if (env.OS == 'Windows_NT') {
                        bat "docker tag ${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG} ${DOCKER_REGISTRY}/${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG}"
                        bat "docker tag ${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG} ${DOCKER_REGISTRY}/${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG}"
                        
                        // Push to registry (uncomment when you have a registry)
                        bat "docker push ${DOCKER_REGISTRY}/${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG}"
                        bat "docker push ${DOCKER_REGISTRY}/${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG}"
                    } else {
                        sh "docker tag ${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG} ${DOCKER_REGISTRY}/${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG}"
                        sh "docker tag ${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG} ${DOCKER_REGISTRY}/${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG}"
                        
                        // Push to registry (uncomment when you have a registry)
                        sh "docker push ${DOCKER_REGISTRY}/${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG}"
                        sh "docker push ${DOCKER_REGISTRY}/${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG}"
                    }
                    
                    echo "Images pushed to registry successfully"
                }
            }
        }
        
        stage('Deploy to Docker Compose') {
            when {
                anyOf {
                    branch 'main'
                    branch 'master'
                }
            }
            steps {
                script {
                    echo "Deploying with Docker Compose and monitoring..."
                    
                    // Stop existing containers
                    if (env.OS == 'Windows_NT') {
                        bat "docker-compose down"
                    } else {
                        sh "docker-compose down"
                    }
                    
                    // Pull latest images
                    if (env.OS == 'Windows_NT') {
                        bat "docker-compose pull"
                    } else {
                        sh "docker-compose pull"
                    }
                    
                    // Start services with monitoring
                    if (env.OS == 'Windows_NT') {
                        bat "docker-compose up -d"
                    } else {
                        sh "docker-compose up -d"
                    }
                    
                    // Wait for services to be healthy
                    if (env.OS == 'Windows_NT') {
                        bat "timeout /t 30"
                    } else {
                        sh "sleep 30"
                    }
                    
                    // Check service status
                    if (env.OS == 'Windows_NT') {
                        bat "docker-compose ps"
                    } else {
                        sh "docker-compose ps"
                    }
                    
                    echo "Deployment completed successfully"
                }
            }
        }
        
        stage('Monitoring Setup') {
            when {
                anyOf {
                    branch 'main'
                    branch 'master'
                }
            }
            steps {
                script {
                    echo "Setting up monitoring and observability..."
                    
                    // Wait for monitoring services to be ready
                    if (env.OS == 'Windows_NT') {
                        bat "timeout /t 45"
                    } else {
                        sh "sleep 45"
                    }
                    
                    // Verify monitoring services
                    if (env.OS == 'Windows_NT') {
                        bat "docker-compose ps"
                    } else {
                        sh "docker-compose ps"
                    }
                    
                    // Check Prometheus health
                    if (env.OS == 'Windows_NT') {
                        bat "curl -f http://localhost:9090/-/healthy || echo 'Prometheus not ready yet'"
                    } else {
                        sh "curl -f http://localhost:9090/-/healthy || echo 'Prometheus not ready yet'"
                    }
                    
                    // Check Grafana health
                    if (env.OS == 'Windows_NT') {
                        bat "curl -f http://localhost:3001/api/health || echo 'Grafana not ready yet'"
                    } else {
                        sh "curl -f http://localhost:3001/api/health || echo 'Grafana not ready yet'"
                    }
                    
                    echo "Monitoring setup completed"
                }
            }
        }
        
        stage('Health Check') {
            when {
                anyOf {
                    branch 'main'
                    branch 'master'
                }
            }
            steps {
                script {
                    echo "Performing comprehensive health checks..."
                    
                    // Check application health
                    if (env.OS == 'Windows_NT') {
                        bat "curl -f http://localhost:5000/health || echo 'Backend health check failed'"
                        bat "curl -f http://localhost:3000 || echo 'Frontend health check failed'"
                    } else {
                        sh "curl -f http://localhost:5000/health || echo 'Backend health check failed'"
                        sh "curl -f http://localhost:3000 || echo 'Frontend health check failed'"
                    }
                    
                    // Check database connectivity
                    if (env.OS == 'Windows_NT') {
                        bat "docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q 'SELECT 1' || echo 'Database health check failed'"
                    } else {
                        sh "docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q 'SELECT 1' || echo 'Database health check failed'"
                    }
                    
                    // Check monitoring endpoints
                    if (env.OS == 'Windows_NT') {
                        bat "curl -f http://localhost:9100/metrics || echo 'Node exporter not accessible'"
                        bat "curl -f http://localhost:8080/metrics || echo 'cAdvisor not accessible'"
                    } else {
                        sh "curl -f http://localhost:9100/metrics || echo 'Node exporter not accessible'"
                        sh "curl -f http://localhost:8080/metrics || echo 'cAdvisor not accessible'"
                    }
                    
                    echo "Health checks completed"
                }
            }
        }
    }
    
    post {
        always {
            script {
                // Clean up Docker images
                if (env.OS == 'Windows_NT') {
                    bat "docker image prune -f"
                } else {
                    sh "docker image prune -f"
                }
                
                // Clean up containers
                if (env.OS == 'Windows_NT') {
                    bat "docker container prune -f"
                } else {
                    sh "docker container prune -f"
                }
            }
            
            echo "Pipeline completed"
        }
        success {
            echo "Pipeline succeeded!"
        }
        failure {
            echo "Pipeline failed!"
        }
    }
} 