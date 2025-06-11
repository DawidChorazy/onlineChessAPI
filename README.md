## API STWORZONE PRZEZ: RADOSŁAW CYBORON, SZYMON ADAMSKI, DAWID CHORĄŻY

# OnlineChessAPI

REST-owe API do zarządzania bazą partii szachowych, napisane w ASP.NET Core 8 (.NET 8).

## Spis treści
1. [Funkcjonalności](#funkcjonalności)
2. [Import danych z CSV](#import-danych-z-csv)
3. [Uruchomienie aplikacji](#uruchomienie-aplikacji)
4. [Autoryzacja](#autoryzacja)
5. [Dokumentacja OpenAPI](#dokumentacja-openapi)

---

## Funkcjonalności

| Endpoint | Dostęp | Opis |
|----------|--------|------|
| `POST /api/Auth/register` | publiczny | Rejestracja użytkownika i natychmiastowe zwrócenie JWT |
| `POST /api/Auth/login` | publiczny | Logowanie (username + password) – zwraca JWT |
| `GET /api/ChessGames` | publiczny | Lista partii z paginacją, sortowaniem i filtrowaniem |
| `GET /api/ChessGames/{id}` | publiczny | Szczegóły wybranej partii |
| `POST /api/ChessGames` | **wymaga JWT** | Dodanie nowej partii |
| `PUT /api/ChessGames/{id}` | **wymaga JWT** | Aktualizacja partii |
| `DELETE /api/ChessGames/{id}` | **wymaga JWT** | Usunięcie partii |

Komentarze (`/api/Comments`) – analogiczne (rozszerzalne).

---

## Import danych z CSV

Plik [DbSeeder.cs] automatycznie wypełnia bazę danymi przy starcie aplikacji (w trybie *Development*).

1. **Użytkownicy** – dodawani programowo z hasłem `P@ssw0rd!` (patrz [SeedUsersAsync]).  
2. **Partie** – ładowane z pliku `chess_games.csv` (kolumny zgodne z datasetem Lichess).  
   * Ścieżka:  `../../../../chess_games.csv`  

3. **Komentarze** – generowane przykładowe do pierwszych 5 partii.