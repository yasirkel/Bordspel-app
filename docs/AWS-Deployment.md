# BordspelApp AWS deployment

Deze configuratie gebruikt één x86-64 Ubuntu EC2-host in `eu-north-1` met Docker Compose. Er is geen publieke SQL Server-poort: alleen TCP 80 en 443 worden in de security group geopend. Caddy verzorgt HTTPS voor `WEB_HOSTNAME` en `API_HOSTNAME`; `sslip.io` kan worden gebruikt met het publieke IPv4-adres, bijvoorbeeld `web.203.0.113.10.sslip.io`.

## Eenmalige hostvoorbereiding

1. Installeer Docker Engine en Docker Compose v2.
2. Installeer de AWS Systems Manager Agent en koppel een instance role met ECR pull- en SSM-rechten.
3. Clone deze repository naar `/opt/bordspel-app`.
4. Maak `/opt/bordspel-app/.env` aan op de host met waarden uit `.env.example`. Dit bestand staat nooit in GitHub.
5. Controleer de security group: 80/443 vanaf internet, SSH alleen vanaf een beheerd IP of gebruik SSM, en geen 1433.

De eerste database-initialisatie gebeurt uitsluitend door `migrator`. De WebApp en API roepen geen `Migrate` of `MigrateAsync` aan tijdens startup.

## GitHub-configuratie

Maak een OIDC trust policy die uitsluitend `repo:yasirkel/Bordspel-app:ref:refs/heads/fix/assessment-completion` toestaat. Gebruik een beperkte role met alleen ECR push en SSM deployment naar de specifieke instance.

Vereiste repository secrets:

- `AWS_ROLE_TO_ASSUME`
- `EC2_INSTANCE_ID`
- `PUBLIC_WEBAPP_URL`
- `PUBLIC_API_URL`

Zet repository variable `AWS_DEPLOY_ENABLED` pas op `true` nadat de host, OIDC role, ECR-repositories en `.env` gecontroleerd zijn. Zonder deze variable draait alleen CI; er wordt dan niet gedeployed.

## Lokale controle

Kopieer `.env.example` naar `.env`, vervang alle voorbeeldwaarden en voer uit:

```text
docker compose config
docker compose build
docker compose up migrator
```

Gebruik nooit de voorbeeldwachtwoorden buiten een tijdelijke lokale testomgeving.
