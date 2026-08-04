# Seed images — drop folder

Put the real photos here. The database seeder will read these files and store them as
image files (not base64), served later through an authenticated download endpoint.

**Nothing is wired up yet.** The seeder does not read this folder today — that happens in
the A6 seed-expansion task. This folder and manifest exist so the images can be collected
in advance. Adding or removing files here changes nothing until A6 lands.

These files **are tracked in git** — the seed needs them present at review time, so do not
add them to `.gitignore`.

---

## Rules that apply to every image

| Rule | Value |
|---|---|
| Formats | `.jpg` (preferred, photos) or `.png` (preferred, logos with transparency) |
| Colour | sRGB |
| Max file size | **200 KB** per file — resize before dropping, don't ship 4 MB camera originals |
| Filenames | exactly as listed below: lowercase, no spaces, no diacritics |
| Licensing | only images we may redistribute — the repo is public at submission |

Filenames are **exact**. The seeder looks them up by name; a typo means a missing image at
review, which is a graded defect (the rules require images in list views and seed data that
includes images).

If a listed file is missing when A6 runs, that row simply seeds without an image. Nothing
crashes, but the screen looks unfinished.

---

## `users/` — profile avatars

**Square, 1:1.** Face roughly centred; the UI crops to a circle, so leave a little margin.
**512 × 512 px**, max 200 KB.

Named after the seeded username.

| File | Seed user | Role |
|---|---|---|
| `admin1.jpg` | Alice Admin | Admin |
| `company1.jpg` | Bella Company | Company |
| `driver1.jpg` | Damir Driver | Driver |
| `passenger1.jpg` | Petra Passenger | Passenger |

A6 expands the seed to roughly 15 users. The extra ones follow the same pattern —
`admin2.jpg`, `driver2.jpg`, `passenger3.jpg`, and so on. Add whatever you have; the four
above are the ones that exist today and matter most, since they are the per-role logins
listed in the README for the reviewer.

## `companies/` — taxi company logos

**Square, 1:1**, `.png` with a transparent background if possible. **512 × 512 px**,
max 200 KB. These sit next to the company name in list views and on the company detail
screen, so they must read clearly at about 40 px.

Named after the company slug. The companies do not exist yet — A6/B1 creates them — so
these names are the proposed seed set. Tell me if you'd rather use different company names
and I'll update this list.

| File | Seed company | City |
|---|---|---|
| `mostar-taxi.png` | Mostar Taxi | Mostar |
| `neretva-prevoz.png` | Neretva Prevoz | Mostar |
| `sarajevo-express.png` | Sarajevo Express | Sarajevo |
| `zuti-taxi.png` | Žuti Taxi | Sarajevo |
| `brzi-taxi.png` | Brzi Taxi | Mostar |

## `cars/` — vehicle photos

**Landscape, 4:3.** **800 × 600 px**, max 200 KB. Whole car in frame, side or
three-quarter view. Generic stock photos are fine — they do not need to match the plate.

Named after the registration plate, lowercased with dashes removed.

| File | Seed car | Company |
|---|---|---|
| `a12t345.jpg` | Škoda Octavia | Mostar Taxi |
| `a34k567.jpg` | VW Passat | Mostar Taxi |
| `e56m789.jpg` | Škoda Superb | Neretva Prevoz |
| `k78j012.jpg` | Toyota Corolla | Sarajevo Express |
| `t90a234.jpg` | Hyundai i30 | Žuti Taxi |

A6 seeds 10+ cars. Five photos is enough to make every list view look populated — reuse
them across the remaining rows if you don't want to source ten distinct images.

---

## Priority if you're short on time

1. `users/` — all four. These are the reviewer's login accounts.
2. `cars/` — at least three. Vehicle lists are the most image-heavy screens.
3. `companies/` — at least two, so the company list isn't entirely placeholder.
