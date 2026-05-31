module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'type-enum': [2, 'always', ['feat', 'fix', 'docs', 'style', 'refactor', 'test', 'chore']],
    'scope-enum': [2, 'always', ['gateway', 'api', 'infrastructure', 'domain', 'application', 'tests', 'docs', 'deps']],
    'subject-max-length': [2, 'always', 72]
  }
};
