# Assessment demo seed

The assessment dataset is created only when the Migrator is explicitly started with `ASSESSMENT_SEED=true`. WebApp and API startup never run this seed.

The procedure is idempotent:

- persons and Identity users are matched by their fixed fictitious email addresses;
- board games are matched by name;
- game nights are matched by title and organizer;
- participants and potluck items are matched by their stable relationships and descriptions.

Run it only against the intended assessment database with six password environment variables supplied at runtime. Password values must never be committed or written to application logs:

```bash
docker compose --env-file .env run --rm \
  -e ASSESSMENT_SEED=true \
  -e ASSESSMENT_DOCENT_PASSWORD \
  -e ASSESSMENT_ORGANIZER_PASSWORD \
  -e ASSESSMENT_PLAYER1_PASSWORD \
  -e ASSESSMENT_PLAYER2_PASSWORD \
  -e ASSESSMENT_PLAYER3_PASSWORD \
  -e ASSESSMENT_MINOR_PASSWORD \
  migrator
```

The command applies pending EF migrations first and then adds the fictitious assessment records. It does not reset or delete either database. Re-running it does not create duplicates.

The docent account receives the existing `Admin` role because the API's administrative endpoints authorize that role. All other seeded users receive the regular `client` role.
