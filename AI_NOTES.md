# AI Usage Notes

## Tools Used

- **Claude (Anthropic)** — primary development assistant for architecture design, scaffolding, and code review
- **GitHub Copilot** — inline suggestions during editing in VS Code

---

## Most Helpful Prompts

### 1. Multi-format date parsing strategy
> "I need to parse date strings in at least three different formats including a human-readable one like 'June 2, 2022' and reject logically invalid dates like April 31 without throwing unhandled exceptions. What's the cleanest approach in C# without a third-party library?"

This surfaced the `DateTime.TryParseExact` with an ordered format array pattern, which is readable, testable, and requires no NuGet dependency. The ordered-formats approach made the validation logic easy to explain in a walkthrough.

### 2. HttpClient registration for .NET 8 minimal API
> "What's the idiomatic way to register a typed HttpClient in .NET 8 Program.cs using the generic AddHttpClient overload, and why is that better than newing up HttpClient directly?"

This prompted the `builder.Services.AddHttpClient<OpenMeteoClient>()` pattern with `HttpClientFactory` under the hood — important for socket pooling and avoiding DNS staleness bugs that would bite in production.

### 3. React sort state with null-safe comparator
> "I have a TypeScript array of objects where some numeric fields can be null. I want a sort function that always sends null values to the bottom regardless of sort direction."

The AI produced the pattern used in `sortEntries()`: check for null before the direction-aware comparison, always returning `+1` for null `a` and `-1` for null `b`.

---

## Where the AI Was Wrong — and How I Caught It

**Issue:** When asked to scaffold the `OpenMeteoResponse` deserialization, the AI initially suggested using `System.Text.Json` default PascalCase property names and mapping them to the snake_case JSON fields via `[JsonPropertyName("temperature_2m_max")]` attributes on every property.

**Why it was wrong:** That works, but it's verbose and brittle — adding a new field means remembering to add the attribute. The API returns consistently snake_case JSON, so the right fix is configuring the serializer once with `JsonNamingPolicy.SnakeCaseLower` on the `JsonSerializerOptions` passed to `ReadFromJsonAsync`.

**How I caught it:** I noticed the attributes were all mechanical repetition and checked whether `System.Text.Json` in .NET 8 supported a global snake_case policy — it does (added in .NET 8). Switching to the policy-based approach reduced the model classes by ~8 lines and made future extensions automatic.

---

## Code Written Without AI Assistance

**`DateParser.TryParse` logic — specifically the April 31 rejection path.**

`DateTime.TryParseExact` with `"MMMM dd, yyyy"` already rejects `April 31` because the BCL validates calendar correctness. However, I chose to explicitly document this behavior with a comment rather than let it be implicit, because during the interview walkthrough it needs to be clear *why* the rejection happens without requiring the interviewer to know BCL internals.

I also wrote the fallback loose-parser round-trip check (`parse → format → re-parse`) myself. The AI suggested just using `DateTime.TryParse` as a catch-all, which would silently convert `April 31` to `May 1`. The round-trip guard prevents that class of silent data mutation.

Writing these paths by hand ensured I can explain every branch under interview conditions.
