# FIT-RS2-2026

## Signing in

Every seeded account uses the password `Test123`.

| Username     | Role      | Signs in with              |
| ------------ | --------- | -------------------------- |
| `admin1`     | Admin     | Password                   |
| `company1`   | Company   | Password                   |
| `driver1`    | Driver    | Password                   |
| `passenger1` | Passenger | Password                   |
| `company4`   | Company   | Password + emailed code    |

`company4` is there to demonstrate two-factor authentication: after the password it asks
for a six-digit code that is emailed to the account, and no token is issued until that
code is entered. Any account can turn the same thing on for itself from the Security
section of the desktop Settings screen.

### Reading the code when no mail server is configured

`SMTP_HOST` is blank in `.env.example`, so the Worker writes each message to its log
instead of sending it. The code is on that log line:

```bash
docker compose logs pickmeup-worker | grep "Your sign-in code is"
```

Set a real `SMTP_HOST` in `.env` and the Worker sends the email for real instead.
