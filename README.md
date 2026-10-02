# Stylish

Flutter app, built following the same architecture/method as the
`nti8_news` reference project (Cubit state management + clean
feature-based structure).

## Structure
```
lib/
  core/
    utils/        -> app_colors.dart, app_assets.dart
    components/    -> reusable widgets (DefaultBtn, DefaultTextField)
    helper/        -> my_navigator.dart (goTo)
  features/
    splash/        -> SplashCubit + SplashView (2s timer)
    onboarding/     -> OnboardingCubit + OnboardingView (Skip/Next/Prev, dot indicator)
    auth/           -> GetStartedView, LoginView (Sign In), RegisterView (Sign Up)
    shop/           -> MainView shell (Home / Items / Profile tabs) + ProductDetailsView
```

## Currently implemented (matches the Figma "shop" flow)
- **Splash Screen** — logo (`assets/images/logo.png`)
- **Onboarding**, 3 slides with real SVG illustrations (Choose Products,
  Make Payment, Get Your Order). "Skip" hidden on the last slide,
  "Prev" hidden on the first, last button reads "Get Started".
- **Get Started** — background photo + Login/Register buttons
- **Sign In / Sign Up** — full forms with validation, password
  visibility toggles, and the highlighted "Register" terms text
- **Home tab** — search bar, categories row, promo banner, Recommended grid
- **Items tab (Trending Products)** — categories row + Products grid
- **Product Details** — big photo, favorite heart, quantity stepper, Add To Cart
- **Bottom nav** (Home / Items / Profile) shared across the shop screens

## TODO
- Add the Profile screen once its Figma design is sent, following the
  same `features/<name>/presentation/{cubit,views}` pattern.
  `ProfileTab` is currently a placeholder.
- Wire up real APIs for products/cart/auth once a backend exists
  (search each file for `// TODO:`).

## Notes on assumptions
- `badge_bag.png` (red bag icon) shows on a product card once it's
  been added to the cart — that was my best guess for that asset's
  role from the Figma layer name; flag it if it was meant for
  something else.
- Product data currently repeats the same "Mens Starry" placeholder
  shirt used across the whole Figma file — swap in real products
  once you have them (`lib/features/shop/data/models/product_model.dart`).
- Prices are shown as `$` everywhere for consistency (the Figma file
  itself mixes `₹` in the grid and `$` on the details screen).

## Run
```
flutter pub get
flutter run
```

## Cart / Checkout / Search / Favorites (latest batch)
- **Cart** — editable Shopping List (qty steppers), auto-computed
  Subtotal / Tax and Fees / Delivery Fee, Checkout button.
- **Checkout** — delivery address field + location button (decorative
  for now, no map integration), read-only Shopping List, Order Total,
  Place Order (clears the cart and returns to Home).
- **Search** — live filter over the product catalog by name/description.
- **My Favorites** — grid of everything favorited across the app.
- The red "bag" badge is a **floating Cart button** (bottom-right,
  shown on the Home and Items tabs), not a per-product-card indicator
  — confirmed from a closer look at the Figma file. It opens `CartView`
  and shows a small count badge when the cart isn't empty.
- Product catalog now has 3 real products (Mens Starry ×4 placeholder
  cards, Women's Casual Wear, Men's Jacket) with discount pricing
  (`originalPrice` shown with strikethrough) matching Cart/Checkout.

## My Orders / Order Details (latest batch)
- **My Orders** — Active / Completed / Cancelled pill tabs, each with
  its own order list and matching empty state.
- **Order Details** — full item breakdown (reusing `CartProductRow`),
  Subtotal / Tax / Delivery / Order Total, and Cancel Order / Track
  Driver buttons (only shown while the order is Active).
- Placing an order in Checkout now creates a real `OrderModel`
  (numbered starting at 005, matching the Figma mock) instead of just
  clearing the cart. Cancelling — from either screen — moves it to
  the Cancelled tab. There's no backend, so orders never
  auto-transition to Completed (see `// TODO:` in `main_cubit.dart`).
- **Profile tab** now has a real (if simple) menu — My Orders, My
  Favorites, Settings (placeholder), Logout — built from the Iconly
  Pro layer names visible in the Figma panel (Bag/Heart/Setting/Logout),
  since the full visual design for Profile hasn't been sent yet.
  Swap this out once you send that screen.

## Profile / My Profile / Settings (latest batch)
- **Profile tab** now matches the real design: avatar, tappable name
  link, My Profile / My Orders / My Favorites / Settings menu (each
  with a chevron), a divider, then Log Out.
- **My Profile** — avatar with an edit badge (decorative, no image
  picker wired up), Full Name + Phone fields, Save (updates the name
  shown on the Profile tab; stored in `MainCubit`, no backend yet).
- **Settings** — Language row with an AR/EN segmented toggle. This is
  visual-only for now — no actual localization/RTL switch is wired
  up (worth flagging if you want that built out).
- No avatar photo asset was provided, so it's a placeholder person
  icon in a grey circle — send the image if you'd like the real one.

## Real API integration (latest batch)
The app is now wired to the real backend instead of mock data:
`https://nti-ecommerce-api-production-0f69.up.railway.app/api/`

### What's connected
- **Auth**: register / login (real network calls, loading spinner on
  the button, error shown via SnackBar), update_profile, logout
  (clears stored tokens).
- **Home / Items**: categories, products, and the banner slider are
  fetched live via `MainCubit.loadHomeData()` (called once when
  `MainView` first opens). Pull-to-refresh is wired up on both tabs.
- **Search**: calls `/products/search` live as you type.
- **Favorites**: `add_to_favorite` is called on toggle (see
  assumption below); tracked locally too so the heart icon stays
  instantly responsive.
- **Cart → Checkout → Orders**: cart stays local (no cart endpoint
  exists), but Place Order calls `POST place_order` with the real
  cart contents, then refreshes the order list from `GET orders`.
  Cancelling calls `POST orders/cancel/{id}`.

### Important — please read
**The Postman collection had no example responses saved**, so the
exact JSON field names (token keys, image path format, list
wrapper keys, error shape, etc.) are *educated guesses*, documented
with `// TODO` comments at each guess — search the codebase for
`TODO` to see all of them. Key files:
- `lib/core/network/api_constants.dart` — image URL resolution
- `lib/core/network/dio_helper.dart` — token refresh flow
- `lib/features/auth/data/repos/auth_repo.dart` — token/user field names
- `lib/features/shop/data/models/*.dart` — each model's `fromJson`

**If something doesn't parse correctly when you run it**, the fastest
fix is: run the request in Postman, copy the actual JSON response,
and send it over — I can correct the exact field name in minutes.

### Known gaps in the API itself (not something I can work around)
- No "list my favorites" or "remove favorite" endpoint — only
  `add_to_favorite`, assumed to toggle.
- No cart endpoint — cart is entirely local until checkout.
- No endpoint to move an order from Active → Completed (only
  `orders/complete/{id}`, which looks admin-only) — so orders placed
  in-app will only ever show as Active or Cancelled.

### Setup
```
flutter pub get
flutter run
```

## API schema — confirmed via live testing (latest)
After a long debugging session (thank you for your patience!), here's
what's now **confirmed working**, not guessed:

- **Auth tokens**: every normal protected endpoint (products,
  categories, orders, favorites, update_profile, etc.) uses the
  **access_token** — standard JWT practice, confirmed by decoding the
  JWTs saved as examples in the Postman collection itself
  (`"type":"access"` in the payload).
- **Exception**: `get_user_data` specifically rejects the access_token
  and demands the **refresh_token** instead (confirmed live — likely a
  backend quirk/bug scoped to just that one route). The app already
  handles this as a special case (`DioHelper.get(..., useRefreshToken:
  true)`), so no action needed there.
- **Images**: the API returns the field as **`image_path`** (not
  `image`), and it's already a full Cloudinary URL — not a relative
  path. `ApiConstants.resolveImageUrl` already handles both cases
  (passes full URLs through as-is).
- **GET products** response shape (confirmed real example):
  ```json
  {
    "products": [
      {
        "id": 1, "name": "...", "description": "...", "price": 80.0,
        "best_seller": 1, "is_favorite": false,
        "image_path": "https://res.cloudinary.com/...",
        "category": {
          "id": 1, "title": "cat 1", "description": "...",
          "image_path": "https://res.cloudinary.com/..."
        }
      }
    ]
  }
  ```
  Categories/sliders are assumed to follow the same `image_path`
  convention (not yet independently confirmed — flag it if
  `categories`/`sliders` come back looking wrong).
- `get_user_data`'s `user.favorite_products` is used to seed the
  in-app favorites set right after login/register.

If products/categories now render correctly in the app, the
remaining unconfirmed area is **orders** (`place_order` / `GET
orders` response shape) — send a real `GET orders` response next if
you want that locked down too.

## Debug console logging (latest)
In debug builds, every API call is now printed to the `flutter run`
terminal — method, URL, auth header (truncated), request body, and
the full response or error. No Postman needed to see what's
happening: just watch the terminal while using the app (login,
register, browsing products, etc.). This is automatically disabled
in release builds.
