# Changelog

## [1.1.0](https://github.com/marcosviniciosmendesdelima/HelpDesk/compare/v1.0.0...v1.1.0) (2026-06-01)


### Features

* adiciona consumidor RabbitMQ para processar tickets e salvar no banco de leitura ([22ac0ae](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/22ac0ae215c51719dc0ea9349c481e0efaa29b68))
* adiciona Redis Backplane ao SignalR na etapa 10.2 ([2ffcee8](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/2ffcee8fb662a6e6493a91e1df391045095bc8c2))
* ajusta infraestrutura do Redis Backplane na Etapa 10.2 ([b5b819c](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/b5b819cd800f21651d5150208d99336643be0d6b))
* arquitetura de microsserviços com Gateway YARP e resiliência Polly ([a61fe5d](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/a61fe5d879c4e8460d9479d985141f9b61695f11))
* conclui logs e resiliencia com codigo 100 por cento limpo de avisos ([1b56b8c](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/1b56b8ca367133de7db8e3388180714aa33b6e6c))
* concluída etapa 8.1 - CQRS Write Side e Integração RabbitMQ ([3a6ebf6](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/3a6ebf642b697871ff6b387624877a4b8bb0471d))
* configurar .dockerignore e ajustar pipeline para SonarCloud ([5ecdb9f](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/5ecdb9f555552e639019841f552dc6e501c07689))
* corrige string de conexao, rotas do yarp proxy e tabela do postgres ([58374eb](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/58374ebec68060faec6d73d8df52552e34e3bb78))
* estrutura completa Clean Architecture organizada por Marcos ([ca943cd](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/ca943cdb427943531a40a4f4fb10fec981a3d047))
* estrutura de banco de dados e correções de ambiente ([fb3d39f](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/fb3d39fd447f978817f9fa2321dba56d6db17806))
* finaliza etapa 9.1 - corrige referencia do worker e injeta redis ([d317e50](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/d317e50842268f303e79aaea965b9ddd4feb7c0d))
* helpdesk gateway + worker com rabbitmq, redis, signalr e serilog ([648d02d](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/648d02d96cbc4abf55dd295cc927df876ef9688a))
* implementa entidade Chamado e value object Prioridade no domínio ([ea4ddef](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/ea4ddef53949aad419599242fbfb61d7018bdb79))
* implementa infraestrutura de banco de dados e conexao sqlalchemy ([3d9a705](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/3d9a705a7b859b2ce8a827ff6dd022161793ea9e))
* implementa infraestrutura do SignalR Hub e push reativo no Worker (Etapa 10.1) ([754cdf3](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/754cdf36d71102e54c42746ea89425550f628530))
* implementa infraestrutura do SignalR Hub e push reativo no Worker (Etapa 10.1) ([83a8634](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/83a86346b31fd61400f3492bcd5ba97d628f1b55))
* implementa padrao Cache-Aside e invalidacao por evento na listagem de tickets (Etapa 9.2) ([a9e743b](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/a9e743b5d3cf757e37488ac075560fe29506d443))
* implementa resiliência com Polly (Retry e Circuit Breaker) no Gateway ([0274403](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/02744035246cfa9c8a99c4d4b97605c5a467f66e))
* implementa Serilog e resiliência com Polly ([074b547](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/074b547881aa464c4071dfc5950a77a825a5db82))
* implementa testes unitários da entidade Chamado (Base da Pirâmide) ([dc8c564](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/dc8c564c10cc067406c98542f491829876420320))
* Infraestrutura RabbitMQ Etapa 7.1 concluída ([c24c1d0](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/c24c1d0b8cec47e546ac731f5cdb7a4034771af6))
* **infrastructure:** configura automação de releases e proteção de branch ([#18](https://github.com/marcosviniciosmendesdelima/HelpDesk/issues/18)) ([50e232f](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/50e232f59ad1955bb2ef1d63bd428240d76fb394))
* integrando API Gateway YARP ao repositório principal ([934e173](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/934e17310f24ed5fa2b54a7a05b28738a0224552))
* organizando Gateway em subpasta e subindo tudo - Marcos Vinícios ([535446c](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/535446c2f2f2e40211d3255d3676f50a5ecde4f0))
* Worker RabbitMQ e integracao Real-Time com SignalR homologados ([c86865e](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/c86865eccedb39e714c07c59375064506c42e21c))


### Bug Fixes

* ajusta rota e contrato do teste de integração (3 passed) ([4189b88](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/4189b88a21186df3ddaa0dbbc1722c053c9d8212))
* ajusta testes unitários para novo campo prioridade ([28f419c](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/28f419c9d89587ef1a1fbf54bfdc041ae18d8b68))
* ajustar pipeline com compatibilidade Node24 e artefatos ([2129f89](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/2129f896cf39a197029f926e418f07f006ead760))
* aplica política de resiliência Polly e logs estruturados no Worker do Postgres ([2bafc35](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/2bafc35cb61544c01cbe2d282f79b7bd30dbf839))
* compilação .NET, bibliotecas MySQL e conexão com RabbitMQ ([1c3a075](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/1c3a07531524d420224cf93a0df0689c56d99f4d))
* corrigindo indentação do docker-compose para funcionamento do ambiente ([0922169](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/09221698ed75ac2140c4010c59809eae2668e02e))
* **infrastructure:** allow tests to fail gracefully and guard Sonar scan ([43da9d8](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/43da9d8a4bedae13ab706526135f9e5ab6ed3796))
* **infrastructure:** consolidate sonar scan in test job and remove broken artifact flow ([775a862](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/775a862c57cbb1ddd60252bf97464ac3e5ca9f8f))
* **infrastructure:** correct GitHub Actions if condition syntax for SONAR_TOKEN ([745add6](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/745add67fa4976698c50d3a8403f8db96032bf87))
* **infrastructure:** emit coverage.opencover.xml for Sonar OpenCover import ([7241f8e](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/7241f8ee2428de5815799572d76143ab9679c26f))
* **infrastructure:** guard SonarCloud workflow when SONAR_TOKEN is missing ([0684d8e](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/0684d8ead21c7d9cf83ad46f90eb206d941e64db))
* **infrastructure:** move Sonar scan to separate job with job-level if condition ([ae05365](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/ae05365199e2245bb76d93e1bd812849b8a51734))
* **infrastructure:** move SONAR_TOKEN check to job-level if condition ([dbf22f0](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/dbf22f0c981320f19ef72079f6588ee329cf8c61))
* **infrastructure:** remove artifact flow and run Sonar scan in same job ([ca4c2b1](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/ca4c2b168c61e0a4b3756d27de619b3231a7c1f0))
* **infrastructure:** upload-artifact path not scan filesystem root (avoid EACCES) ([5b75196](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/5b751967fe031bcf546380b465252eb6a03c1105))


### Performance Improvements

* rota do gateway otimizada para 200 OK ([08e0359](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/08e035910eb9e5e5a1d61da14bd656d9b2da9309))

## 1.0.0 (2026-05-31)


### Features

* adiciona consumidor RabbitMQ para processar tickets e salvar no banco de leitura ([22ac0ae](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/22ac0ae215c51719dc0ea9349c481e0efaa29b68))
* adiciona Redis Backplane ao SignalR na etapa 10.2 ([2ffcee8](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/2ffcee8fb662a6e6493a91e1df391045095bc8c2))
* ajusta infraestrutura do Redis Backplane na Etapa 10.2 ([b5b819c](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/b5b819cd800f21651d5150208d99336643be0d6b))
* arquitetura de microsserviços com Gateway YARP e resiliência Polly ([a61fe5d](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/a61fe5d879c4e8460d9479d985141f9b61695f11))
* conclui logs e resiliencia com codigo 100 por cento limpo de avisos ([1b56b8c](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/1b56b8ca367133de7db8e3388180714aa33b6e6c))
* concluída etapa 8.1 - CQRS Write Side e Integração RabbitMQ ([3a6ebf6](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/3a6ebf642b697871ff6b387624877a4b8bb0471d))
* corrige string de conexao, rotas do yarp proxy e tabela do postgres ([58374eb](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/58374ebec68060faec6d73d8df52552e34e3bb78))
* estrutura completa Clean Architecture organizada por Marcos ([ca943cd](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/ca943cdb427943531a40a4f4fb10fec981a3d047))
* estrutura de banco de dados e correções de ambiente ([fb3d39f](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/fb3d39fd447f978817f9fa2321dba56d6db17806))
* finaliza etapa 9.1 - corrige referencia do worker e injeta redis ([d317e50](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/d317e50842268f303e79aaea965b9ddd4feb7c0d))
* helpdesk gateway + worker com rabbitmq, redis, signalr e serilog ([648d02d](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/648d02d96cbc4abf55dd295cc927df876ef9688a))
* implementa entidade Chamado e value object Prioridade no domínio ([ea4ddef](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/ea4ddef53949aad419599242fbfb61d7018bdb79))
* implementa infraestrutura de banco de dados e conexao sqlalchemy ([3d9a705](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/3d9a705a7b859b2ce8a827ff6dd022161793ea9e))
* implementa infraestrutura do SignalR Hub e push reativo no Worker (Etapa 10.1) ([754cdf3](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/754cdf36d71102e54c42746ea89425550f628530))
* implementa infraestrutura do SignalR Hub e push reativo no Worker (Etapa 10.1) ([83a8634](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/83a86346b31fd61400f3492bcd5ba97d628f1b55))
* implementa padrao Cache-Aside e invalidacao por evento na listagem de tickets (Etapa 9.2) ([a9e743b](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/a9e743b5d3cf757e37488ac075560fe29506d443))
* implementa resiliência com Polly (Retry e Circuit Breaker) no Gateway ([0274403](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/02744035246cfa9c8a99c4d4b97605c5a467f66e))
* implementa Serilog e resiliência com Polly ([074b547](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/074b547881aa464c4071dfc5950a77a825a5db82))
* implementa testes unitários da entidade Chamado (Base da Pirâmide) ([dc8c564](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/dc8c564c10cc067406c98542f491829876420320))
* Infraestrutura RabbitMQ Etapa 7.1 concluída ([c24c1d0](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/c24c1d0b8cec47e546ac731f5cdb7a4034771af6))
* **infrastructure:** configura automação de releases e proteção de branch ([#18](https://github.com/marcosviniciosmendesdelima/HelpDesk/issues/18)) ([50e232f](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/50e232f59ad1955bb2ef1d63bd428240d76fb394))
* integrando API Gateway YARP ao repositório principal ([934e173](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/934e17310f24ed5fa2b54a7a05b28738a0224552))
* organizando Gateway em subpasta e subindo tudo - Marcos Vinícios ([535446c](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/535446c2f2f2e40211d3255d3676f50a5ecde4f0))
* Worker RabbitMQ e integracao Real-Time com SignalR homologados ([c86865e](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/c86865eccedb39e714c07c59375064506c42e21c))


### Bug Fixes

* ajusta rota e contrato do teste de integração (3 passed) ([4189b88](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/4189b88a21186df3ddaa0dbbc1722c053c9d8212))
* ajusta testes unitários para novo campo prioridade ([28f419c](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/28f419c9d89587ef1a1fbf54bfdc041ae18d8b68))
* aplica política de resiliência Polly e logs estruturados no Worker do Postgres ([2bafc35](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/2bafc35cb61544c01cbe2d282f79b7bd30dbf839))
* compilação .NET, bibliotecas MySQL e conexão com RabbitMQ ([1c3a075](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/1c3a07531524d420224cf93a0df0689c56d99f4d))
* corrigindo indentação do docker-compose para funcionamento do ambiente ([0922169](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/09221698ed75ac2140c4010c59809eae2668e02e))


### Performance Improvements

* rota do gateway otimizada para 200 OK ([08e0359](https://github.com/marcosviniciosmendesdelima/HelpDesk/commit/08e035910eb9e5e5a1d61da14bd656d9b2da9309))
