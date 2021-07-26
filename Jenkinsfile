pipeline {
    agent any
    environment {
        REPO_API = "environmentmonitors/environmentmonitors-api"
        REPO_POLLER = "environmentmonitors/environmentmonitors-poller"
        TAG = "${BUILD_TIMESTAMP}"
    }
    stages {
        stage('Git clone') {
            steps {
                git branch: 'main',
                    url: 'https://github.com/LivingSkySchoolDivision/EnvironmentSensorDashboard.git'
            }
        }
        stage('Docker build - API') {
            steps {
                dir("src") {
                    sh "docker build -f Dockerfile-API -t ${PRIVATE_DOCKER_REGISTRY}/${REPO_API}:latest -t ${PRIVATE_DOCKER_REGISTRY}/${REPO_API}:${TAG} ."
                }
            }
        }
        stage('Docker build - Poller') {
            steps {
                dir("src") {
                    sh "docker build -f Dockerfile-Poller -t ${PRIVATE_DOCKER_REGISTRY}/${REPO_POLLER}:latest -t ${PRIVATE_DOCKER_REGISTRY}/${REPO_POLLER}:${TAG} ."
                }
            }
        }
        stage('Docker push') {
            steps {
                sh "docker push ${PRIVATE_DOCKER_REGISTRY}/${REPO_API}:${TAG}"
                sh "docker push ${PRIVATE_DOCKER_REGISTRY}/${REPO_API}:latest"
                sh "docker push ${PRIVATE_DOCKER_REGISTRY}/${REPO_POLLER}:${TAG}"
                sh "docker push ${PRIVATE_DOCKER_REGISTRY}/${REPO_POLLER}:latest"
            }
        }
    }
    post {
        always {
            deleteDir()
        }
    }
}