# BordspelApp AWS deployment

Deze configuratie gebruikt één x86-64 Ubuntu EC2-host in `eu-north-1` met Docker Compose. Er is geen publieke SQL Server-poort: alleen TCP 80 en 443 worden in de security group geopend. Caddy verzorgt HTTPS voor `WEB_HOSTNAME` en `API_HOSTNAME`; `sslip.io` kan worden gebruikt met het publieke IPv4-adres, bijvoorbeeld `web.203.0.113.10.sslip.io`.

## Eenmalige hostvoorbereiding

1. Installeer Docker Engine en Docker Compose v2.
2. Installeer de AWS Systems Manager Agent en koppel een instance role met ECR pull- en SSM-rechten.
3. Clone deze repository naar `/opt/bordspel-app`.
4. Maak `/opt/bordspel-app/.env` aan op de host met waarden uit `.env.example`. Dit bestand staat nooit in GitHub.
5. Controleer de security group: 80/443 vanaf internet, SSH alleen vanaf een beheerd IP of gebruik SSM, en geen 1433.

De eerste database-initialisatie gebeurt uitsluitend door `migrator`. De WebApp en API roepen geen `Migrate` of `MigrateAsync` aan tijdens startup.

## Stoppen en starten

De lokale scripts stoppen of starten alleen de bestaande instance; ze termineren niets:

```powershell
.\deploy\aws-stop.ps1 -InstanceId i-xxxxxxxxxxxxxxxxx
.\deploy\aws-start.ps1 -InstanceId i-xxxxxxxxxxxxxxxxx
```

De SQL-data staat in de named Docker-volume `sqlserver-data`, die op de EBS-schijf van de host staat. Configureer op de host `deploy/bordspelapp-compose.service` als systemd-service. Docker gebruikt daarnaast `restart: unless-stopped` voor SQL Server, API, WebApp en Caddy.

De migrator wordt niet door systemd gestart. Database-migraties blijven uitsluitend onderdeel van de GitHub Actions-deployment. De systemd-service start alleen reeds bestaande runtime-containers en voert daarna de interne healthchecks uit.

Omdat er geen Elastic IP wordt gebruikt, kan het publieke IP na een start veranderen. Voer op de host dan uit:

```bash
sudo /opt/bordspel-app/deploy/refresh-sslip-hostnames.sh
```

Daarna veranderen de URL's naar `https://web.<nieuw-ip>.sslip.io` en `https://api.<nieuw-ip>.sslip.io`. GitHub CI blijft ook succesvol draaien wanneer de instance gestopt is; de deployjob detecteert de status en slaat deployment over.

## GitHub-configuratie

De deployjob gebruikt `environment: production`. Daardoor is de relevante legacy OIDC-subject:

```text
repo:yasirkel/Bordspel-app:environment:production
```

Een branch-subject (`...:ref:refs/heads/fix/assessment-completion`) hoort bij een job zonder environment. De GitHub-documentatie beschrijft dat een environment de subject claim wijzigt naar `environment:production`; configureer daarom de AWS trust policy op de environment-subject en beperk de GitHub `production` environment via branch protection tot `fix/assessment-completion`. Zie de [GitHub OIDC-documentatie](https://docs.github.com/en/actions/how-tos/secure-your-work/security-harden-deployments/oidc-in-aws).

De minimale trust conditions zijn conceptueel:

```json
"token.actions.githubusercontent.com:aud": "https://github.com/yasirkel",
"token.actions.githubusercontent.com:sub": "repo:yasirkel/Bordspel-app:environment:production"
```

Controleer eerst of de repository immutable OIDC-subjects gebruikt. GitHub-repositories die na 15 juli 2026 zijn aangemaakt of hiervoor hebben gekozen gebruiken owner- en repository-ID's in de subject. Gebruik in dat geval de exact door GitHub getoonde immutable subjectwaarde.

De workflow bevat daarnaast een expliciete ref-check, zodat ook een handmatige workflow-run vanaf een andere branch niet kan deployen.

De workflow haalt op de EC2-host bij iedere deployment eerst de branch op. Daardoor worden wijzigingen in `docker-compose.yml`, `Caddyfile`, scripts en andere tracked deploymentbestanden daadwerkelijk bijgewerkt voordat de containers worden gestart.

Gebruik een beperkte role met alleen ECR push en SSM deployment naar de specifieke instance.

De workflow bevat voor deze ene assessment-host de niet-geheime waarden voor de OIDC-role, instance-ID en publieke health-URL's. De OIDC-role zelf bevat de branch/environment-beperking; er wordt geen private key of AWS access key gebruikt. Verplaats deze waarden desgewenst later naar repository variables, maar zet geen credentials in Git.

Deployment staat voor deze assessment-branch standaard aan zodra de OIDC-role en host zijn voorbereid. Zet repository variable `AWS_DEPLOY_ENABLED` op `false` om deployment tijdelijk te pauzeren; CI blijft dan builden en testen.

## Lokale controle

Kopieer `.env.example` naar `.env`, vervang alle voorbeeldwaarden en voer uit:

```text
docker compose config
docker compose build
docker compose up migrator
```

Gebruik nooit de voorbeeldwachtwoorden buiten een tijdelijke lokale testomgeving.
