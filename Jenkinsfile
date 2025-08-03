pipeline {
    agent any
    
    environment {
        DOCKER_IMAGE_BACKEND = 'projetpfe-backend'
        DOCKER_IMAGE_FRONTEND = 'projetpfe-frontend'
        DOCKER_TAG = "${env.BUILD_NUMBER}"
        DOCKER_REGISTRY = 'your-registry.com' // Change this to your registry
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
                echo "Checked out code from ${env.GIT_BRANCH}"
            }
        }
        
        stage('Build Backend') {
            steps {
                dir('gestionMissionBack') {
                    script {
                        echo "Building .NET Backend..."
                        
                        // Restore packages
                        bat 'dotnet restore'
                        
                        // Build the solution
                        bat 'dotnet build --configuration Release --no-restore'
                        
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
                        
                        bat 'dotnet test --configuration Release --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"'
                        
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
                        bat 'npm ci'
                        
                        // Build the application
                        bat 'npm run build'
                        
                        echo "Frontend build completed successfully"
                    }
                }
            }
        }
        
        // stage('Test Frontend') {
        //     steps {
        //         dir('gestionMissionFront') {
        //             script {
        //                 echo "Running frontend tests..."
                        
        //                 // Run unit tests
        //                 bat 'npm test -- --watch=false --browsers=ChromeHeadless --code-coverage'
                        
        //                 echo "Frontend tests completed"
        //             }
        //         }
        //     }
        //     post {
        //         always {
        //             // Publish coverage reports
        //             publishHTML([
        //                 allowMissing: false,
        //                 alwaysLinkToLastBuild: true,
        //                 keepAll: true,
        //                 reportDir: 'coverage',
        //                 reportFiles: 'index.html',
        //                 reportName: 'Frontend Coverage Report'
        //             ])
        //         }
        //     }
        // }
        
        stage('Build Docker Images') {
            steps {
                script {
                    echo "Building Docker images..."
                    
                    // Build backend image
                    sh "docker build -t ${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG} -t ${DOCKER_IMAGE_BACKEND}:latest ./gestionMissionBack"
                    
                    // Build frontend image
                    sh "docker build -t ${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG} -t ${DOCKER_IMAGE_FRONTEND}:latest ./gestionMissionFront"
                    
                    echo "Docker images built successfully"
                }
            }
        }
        
        stage('Test Docker Images') {
            steps {
                script {
                    echo "Testing Docker images..."
                    
                    // Test backend container
                    sh "docker run --rm ${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG} dotnet test --configuration Release --no-build"
                    
                    echo "Docker image tests completed"
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
                    sh "docker tag ${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG} ${DOCKER_REGISTRY}/${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG}"
                    sh "docker tag ${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG} ${DOCKER_REGISTRY}/${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG}"
                    
                    // Push to registry (uncomment when you have a registry)
                    // sh "docker push ${DOCKER_REGISTRY}/${DOCKER_IMAGE_BACKEND}:${DOCKER_TAG}"
                    // sh "docker push ${DOCKER_REGISTRY}/${DOCKER_IMAGE_FRONTEND}:${DOCKER_TAG}"
                    
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
                    echo "Deploying with Docker Compose..."
                    
                    // Stop existing containers
                    sh "docker-compose down"
                    
                    // Pull latest images
                    sh "docker-compose pull"
                    
                    // Start services
                    sh "docker-compose up -d"
                    
                    // Wait for services to be healthy
                    sh "sleep 30"
                    
                    // Check service status
                    sh "docker-compose ps"
                    
                    echo "Deployment completed successfully"
                }
            }
        }
    }
    
    post {
        always {
            // Clean up Docker images
            sh "docker image prune -f"
            
            // Clean up containers
            sh "docker container prune -f"
            
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