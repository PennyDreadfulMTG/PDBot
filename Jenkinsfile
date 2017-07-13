node {
    stage('Clone') {
      checkout scm
	  }

    stage('Build') {
		  msbuild()
    }

		stage('Test'){
			mono('Tests/bin/Debug/Tests.exe','')
			//junit "TestResult.xml"
		}

    stage('Archive') {
      archive '**/bin/**/'
    }

	  stage('Post-Build') {
	    step([$class: 'WarningsPublisher', canComputeNew: false, canResolveRelativePaths: false, consoleParsers: [[parserName: 'MSBuild']], defaultEncoding: '', excludePattern: '', healthy: '', includePattern: '', messagesPattern: '', unHealthy: ''])
	  }
}
