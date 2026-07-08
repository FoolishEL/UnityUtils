# UnityUtils

[![Unity Version](https://img.shields.io/badge/Unity-2022.3+-blue.svg)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Набор полезных утилит и инструментов для Unity (Editor + Runtime).

## Установка

Добавьте пакет через Package Manager → **Add package from git URL**:
https://github.com/FoolishEL/UnityUtils.git

## Основные инструменты

### Scene Browser
`Tools → Developer → Scene Browser` (или "on front")

Быстрый браузер сцен из **Build Settings** + возможность добавлять внешние сцены.

- Кнопки: **Open**, **Play** (с автоматическим возвратом), **Ping**.
- Поддержка Play Mode.

### Общий вид
<img width="600" alt="Scene Browser" src="https://github.com/user-attachments/assets/1d402f34-2cef-4a63-a9a0-18a09b030321" />

### Добавление сторонних сцен
<img src="https://github.com/user-attachments/assets/7f07bbfa-f63c-438c-847d-31ac8ec2a03c" width="600" alt="ExternalScenes">

### Bookmarks Window
`Tools → Developer → Bookmark Window Browser`

Удобное хранилище закладок на любые ассеты с группировкой.

- Drag & Drop ассетов
- Группы (создание, переименование, удаление)
- Ping / Open / Move / Delete

<img width="337" alt="Bookmarks" src="https://github.com/user-attachments/assets/1df223c6-85fc-46d4-b9e8-2e693a9bbd28" />

### Texture Utilities
`Tools → Developer → TextureUtilities`

Набор инструментов для работы с текстурами:

- **Slicer** — разрезание спрайтов на отдельные текстуры
- **Merger** — объединение текстур одинакового размера (горизонтально/вертикально)
- **Packer** — создание `Texture2DArray`
- **Add Alpha** — добавление прозрачности по краям

 ### Slicer
<img width="600" alt="Slicer" src="https://github.com/user-attachments/assets/0128c1d2-a88a-4962-ac84-e5a8a98b92ad" />

### Merger
<img width="600" alt="Merger" src="https://github.com/user-attachments/assets/4254460c-ec55-4380-8ecf-bc6acd3db06a" />

### Другие возможности

- **Runtime утилиты**: Singletons, Initable-объекты, SceneReference, UI-хелперы (ButtonHandler и др.)
- **ScriptableObject Creator**
- **Custom Editors** и настройки
- Шаблоны скриптов


