# PickMeUp

PickMeUp is a taxi ordering platform.

A passenger opens the mobile app, picks a pickup point and a destination on a map, and
either orders from a specific taxi company or lets the platform dispatch the ride. A
driver receives the request, accepts it, drives the passenger, and closes the ride. The
passenger pays by card and leaves a review. Taxi companies manage their drivers,
vehicles, shifts and earnings from a desktop application, and an administrator oversees
every company, user and support ticket on the same desktop application.

## What the four roles can do

**Passenger** (mobile). Order a taxi by tapping the map or searching an address, watch
the ride status update live, pay by card through Stripe, read personalised company
recommendations with a written explanation of why each one was suggested, browse ride
and payment history, leave reviews, cancel a ride with a reason, open support tickets,
receive live notifications, and manage the profile including the photo, the password and
two factor sign in.

**Driver** (mobile). Start and stop a shift, take and end breaks, see the ride requests
available to that driver, accept or reject them, run the active ride from pickup to
completion, report the current position so the employer can follow the fleet, review
completed rides and received ratings, open support tickets, and resign from the company.

**Taxi company** (desktop). Maintain the company profile and logo, manage drivers,
vehicles and vehicle assignments, follow orders and shifts, read reviews, review
payments and refunds, watch the fleet on a live map, generate a business report as PDF,
and contact support.

**Administrator** (desktop). Manage taxi companies and users, answer support tickets from
every role, maintain the reference data used across the platform (cities, vehicle
categories, order statuses, payment statuses, ticket statuses), and generate two PDF
reports covering orders by period and the best performing companies.

## How it is built

| Layer | Technology |
| ----- | ---------- |
| API | ASP.NET Core 9 Web API, layered as WebAPI to Services to Model |
| Background service | A separate .NET Worker process consuming RabbitMQ |
| Database | PostgreSQL 16 accessed with EF Core and Npgsql |
| Messaging | RabbitMQ, topic exchange `pickmeup.events` |
| Live updates | SignalR hub at `/hubs/notifications` |
| Payments | Stripe PaymentIntents confirmed by webhook |
| Reporting | QuestPDF |
| Clients | Flutter, one desktop application and one mobile application |
| Maps | flutter_map with OpenStreetMap tiles and Nominatim geocoding |

The API never trusts a client with money or with state. A fare is computed on the server
from the company price list and the logged distance, so no amount sent by a client is ever
used. A payment becomes `Paid` only after Stripe confirms it through the webhook. Every
order status change passes through one centralized state machine that records who changed
it, when, and why, and cancelling a ride always requires a reason.

The Worker is a genuinely separate project and container. It sends the two factor codes,
the password reset codes and the ride notification emails, retrying failed deliveries
with a backoff of one, two, four and eight seconds.

Both Flutter applications route by role after login, so one mobile binary serves
passengers and drivers, and one desktop binary serves companies and administrators.

## Repository layout

```
PickMeUp/
  PickMeUp.WebAPI/          REST API, controllers, filters, SignalR hub
  PickMeUp.Services/        business logic, EF Core DbContext, entities, migrations
  PickMeUp.Model/           DTOs only, no dependencies
  PickMeUp.Common.Services/ cryptography and strongly typed configuration
  PickMeUp.Worker/          RabbitMQ consumer, email delivery
  UI/pickmeup_desktop/      Flutter desktop app (Administrator + Taxi company)
  UI/pickmeup_mobile/       Flutter mobile app (Passenger + Driver)
  seed-assets/              seed images copied into the API image at build time
  docker-compose.yml
  .env.example
```

## Running the platform

### Prerequisites

Docker Desktop, the Flutter SDK, and an Android emulator or device for the mobile
application. Running the API outside Docker additionally needs the .NET 9 SDK.

### 1. Configuration

All configuration lives in a single `.env` file next to `PickMeUp.sln`. Copy the
committed template and fill it in.

```bash
cd PickMeUp
cp .env.example .env
```

`appsettings.json` deliberately holds nothing but logging settings. The database is named
`200051_200103` after both student index numbers.

### 2. Start the backend

```bash
docker compose up --build
```

That single command starts PostgreSQL, RabbitMQ, the API and the Worker. Database
migrations and the seed data are applied automatically on first start. The API listens on
port **5121**, Swagger is at `http://localhost:5121/swagger`, and the RabbitMQ management
console is at `http://localhost:15672`.

### 3. Start the desktop application

```bash
cd PickMeUp/UI/pickmeup_desktop
flutter pub get
flutter run --dart-define=API_BASE_URL=http://localhost:5121/
```

### 4. Start the mobile application

With an Android emulator already running:

```bash
cd PickMeUp/UI/pickmeup_mobile
flutter pub get
flutter run
```

The mobile application already defaults to `http://10.0.2.2:5121/`, which is how the
Android emulator reaches the host machine, so no define is needed there. On a physical
device pass the address of your computer instead, for example
`--dart-define=API_BASE_URL=http://192.168.1.10:5121/`.

### Running the API without Docker

Useful while developing. Start only the infrastructure containers, then run the API from
source. Note the different port.

```bash
cd PickMeUp
docker compose up -d pickmeup-db pickmeup-rabbitmq
dotnet run --project PickMeUp.WebAPI --urls http://localhost:5126
```

Point the clients at `http://localhost:5126/` in that case. The container image copies
`seed-assets/seed-storage` into its storage folder, so when you run from source copy that
folder to `PickMeUp.WebAPI/storage` yourself if you want the seeded photos and logos to
appear.

Entity Framework migrations live in the Services project while the API is the design time
host:

```bash
dotnet ef migrations add <Name> --project PickMeUp.Services --startup-project PickMeUp.WebAPI
dotnet ef database update --project PickMeUp.Services --startup-project PickMeUp.WebAPI
```

## Signing in

Every seeded account uses the password `Test123`.

| Username | Role | Application | Signs in with |
| -------- | ---- | ----------- | ------------- |
| `admin1` | Administrator | Desktop | Password |
| `company1` | Taxi company | Desktop | Password |
| `company4` | Taxi company | Desktop | Password and an emailed code |
| `driver1` | Driver | Mobile | Password |
| `passenger1` | Passenger | Mobile | Password |

The seed also contains `company2`, `company3`, `driver2` through `driver5`, `passenger2`
and `passenger3` so that lists, reports and the recommender have realistic data to work
with.

`company4` demonstrates two factor authentication. After the password it asks for a six
digit code that is emailed to the account, and no token is issued until that code is
entered. Any account can switch the same protection on for itself from the security
section of its settings screen.

### Reading emailed codes when no mail server is configured

`SMTP_HOST` is empty in `.env.example`, so the Worker writes each message to its log
instead of sending it. Both the sign in code and the password reset code appear there:

```bash
docker compose logs pickmeup-worker | grep "code is"
```

Set a real `SMTP_HOST` in `.env` and the Worker sends genuine email instead.

## Card payments

Payments run against Stripe in test mode. Put your test keys into `STRIPE_PUBLISHABLE_KEY`
and `STRIPE_SECRET_KEY`, then forward webhook events to the API so a payment can reach the
`Paid` state:

```bash
stripe listen --forward-to http://localhost:5121/Payments/Webhook
```

Copy the signing secret that command prints into `STRIPE_WEBHOOK_SECRET` and restart the
API. Use card number `4242 4242 4242 4242` with any future expiry date and any CVC. A
declined card leaves the ride intact, so the passenger can simply try paying again.

## Tests and static analysis

```bash
cd PickMeUp/UI/pickmeup_mobile && flutter analyze && flutter test
cd PickMeUp/UI/pickmeup_desktop && flutter analyze && flutter test
```
