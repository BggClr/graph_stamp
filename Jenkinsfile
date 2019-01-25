properties([buildDiscarder(logRotator(numToKeepStr: '5')), disableConcurrentBuilds()])

def artifactsUrl = 'ssh://git-codecommit.us-east-1.amazonaws.com/v1/repos/graph_stamp-artifacts'

node('che-1') {
	def repoPath = 'repo'
	def artifactsPath = 'artifacts'
	def commitMessage = 'auto'
	def buildTimestamp = new Date().format('yyyyMMddHHmmss')

	subst {
		stage ('Prepare') {
			dir(repoPath) {
				checkout poll: false, changelog: false, scm: scm
				commitMessage = bat (script: '@git log -1 --pretty=%%B', returnStdout: true).trim()
			}
			dir(artifactsPath) {
				checkout([
					$class: 'GitSCM',
					branches: [[name: "*/${env.BRANCH_NAME}"]],
					doGenerateSubmoduleConfigurations: false,
					extensions: [
						[$class: 'CloneOption', depth: 1, noTags: true, shallow: true, timeout: 40],
						[$class: 'LocalBranch', depth: 1, localBranch: env.BRANCH_NAME]
					],
					userRemoteConfigs: [[url: artifactsUrl, refspec: "+refs/heads/${env.BRANCH_NAME}:refs/remotes/origin/${env.BRANCH_NAME}"]]
				])
			}
		}

		stage ('Build') {
			dir(repoPath) {
				try {
					bat "build.cmd -target ..\\$artifactsPath"
				} catch(ex) {
					throw ex
				}
			}
		}

		stage ('Publish') {
			dir(artifactsPath) {
				withEnv(["buildTimestamp=$buildTimestamp", "commitMessage=$commitMessage"]) {
					bat 'git add -A --force'
					bat 'git diff --quiet --exit-code --cached || git commit -m "%BUILD_NUMBER% // %buildTimestamp% // %commitMessage%"'
					bat 'git push origin %BRANCH_NAME% --force'
				}
			}
		}
	}
}