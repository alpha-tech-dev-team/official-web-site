
## Запуск

```bash
cd cms
bash init-env.sh              

ddocker compose up -d --build
docker compose ps
```
Админка: http://localhost:1337/admin
При первом запуске Strapi попросит создать администратора.

Создай новость: Content Manager → News item → Create new entry, заполни поля и нажми Publish. Потом открой публичное чтение: Settings → Users & Permissions → Roles → Public → News-item → поставь `find` и `findOne` → Save. Без этого фронт получит `403`.

### 2. API

```powershell
cd src
dotnet run
```

API будет на `http://localhost:5081`. Логин админа по умолчанию — `admin / admin12345`

### 3. Фронтенд

```powershell
cd web
npm install
npm run dev

Открывай `http://localhost:5173`.

## Остановка

```bash
docker compose down        # остановить, данные сохраняются
docker compose down -v     # остановить и удалить данные (база и загрузки)
```

