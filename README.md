Проект представляет собой аналог Trello в части самого базового функционала (создание пространства с досками, в которых находятся колонки с задачами)

**Моими целями на этот проект являются:**
##### Бекенд:
###### Технологии: ASP.NET, JWT, PostgreSQL, Dapper
- Реализация полноценной аутентификации через JWT.
- Построение бекенда следуя принципам чистой архитектуры.
- Изучение SQL и PostgreSQL через самостоятельное написание запросов (Без EF core) и их обработку в Dapper.
- Покрытие кода тестами

##### Фронтенд:
###### Технологии: React, Zustand, dnd
Сделан с минимальным усилием, так как основой этого проекта для меня является бекенд и взаимодействие с ним, не UI.

##### Архитектура бекенда:
Проект организован с разделением ответственности, поддерживается принцип SOLID, активно используется DI, абстракции и слои доступа к данным.

##### Структура проекта:
```
Kanba/
├── Controllers/
├── Helpers/
├── Middleware/
├── Models/
├── Repositories/
│   ├── Abstract/
│   └── Interfaces/
```

##### Планы
- [ ] Авторизация
- [ ] Полное покрытие кода тестами
- [ ] Пользоваться Issue-Driven подходом
- [ ] Выделить ветку dev

##### Запуск проекта

`git clone https://github.com/albelhub/kanba && cd kanba && docker compose up`
