-- ===================================================================
-- Учебная практика УП.01.01
-- Студент: Корж Сергей, группа 4ИСИП-523
-- Тема: Проектирование и разработка базы данных для платформы по проведению опросов и голосований
-- База данных: DB_Korzh_Polls
-- ===================================================================

USE master;
GO

IF DB_ID('DB_Korzh_Polls') IS NOT NULL
BEGIN
    ALTER DATABASE DB_Korzh_Polls SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DB_Korzh_Polls;
END
GO

CREATE DATABASE DB_Korzh_Polls COLLATE Cyrillic_General_CI_AS;
GO

USE DB_Korzh_Polls;
GO

-- 1. Таблица пользователей (user_account)
CREATE TABLE [user_account] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [login] nvarchar(100) NOT NULL UNIQUE,
    [password_hash] nvarchar(255) NOT NULL,
    [role] nvarchar(50) NOT NULL,
    [full_name] nvarchar(255) NOT NULL,
    [email] nvarchar(255) NULL,
    [phone] nvarchar(50) NULL,
    [photo] nvarchar(max) NULL
);
GO

-- 2. Таблица категорий (category)
CREATE TABLE [category] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] nvarchar(255) NOT NULL
);
GO

-- 3. Таблица опросов и голосований (poll)
CREATE TABLE [poll] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [user_id] int NOT NULL,
    [category_id] int NOT NULL,
    [title] nvarchar(255) NOT NULL,
    [description] nvarchar(max) NULL,
    [created_at] datetime NOT NULL DEFAULT GETDATE(),
    [status] nvarchar(50) NOT NULL DEFAULT N'Активен',
    [is_anonymous] bit NOT NULL DEFAULT 0,
    CONSTRAINT FK_Poll_User FOREIGN KEY ([user_id]) REFERENCES [user_account] ([id]) ON DELETE CASCADE,
    CONSTRAINT FK_Poll_Category FOREIGN KEY ([category_id]) REFERENCES [category] ([id]) ON DELETE CASCADE
);
GO

-- 4. Таблица вопросов (question)
CREATE TABLE [question] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [poll_id] int NOT NULL,
    [text] nvarchar(max) NOT NULL,
    [type] nvarchar(50) NOT NULL DEFAULT N'Одиночный выбор',
    [order_number] int NOT NULL DEFAULT 1,
    CONSTRAINT FK_Question_Poll FOREIGN KEY ([poll_id]) REFERENCES [poll] ([id]) ON DELETE CASCADE
);
GO

-- 5. Таблица вариантов ответа к вопросу (question_option)
CREATE TABLE [question_option] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [question_id] int NOT NULL,
    [text] nvarchar(255) NOT NULL,
    CONSTRAINT FK_QuestionOption_Question FOREIGN KEY ([question_id]) REFERENCES [question] ([id]) ON DELETE CASCADE
);
GO

-- 6. Таблица откликов / сессий прохождения (response)
CREATE TABLE [response] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [poll_id] int NOT NULL,
    [user_id] int NOT NULL,
    [created_at] datetime NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Response_Poll FOREIGN KEY ([poll_id]) REFERENCES [poll] ([id]) ON DELETE NO ACTION,
    CONSTRAINT FK_Response_User FOREIGN KEY ([user_id]) REFERENCES [user_account] ([id]) ON DELETE CASCADE
);
GO

-- 7. Таблица конкретных ответов (answer)
CREATE TABLE [answer] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [response_id] int NOT NULL,
    [question_id] int NOT NULL,
    [option_id] int NULL,
    [text_answer] nvarchar(max) NULL,
    CONSTRAINT FK_Answer_Response FOREIGN KEY ([response_id]) REFERENCES [response] ([id]) ON DELETE CASCADE,
    CONSTRAINT FK_Answer_Question FOREIGN KEY ([question_id]) REFERENCES [question] ([id]) ON DELETE NO ACTION,
    CONSTRAINT FK_Answer_Option FOREIGN KEY ([option_id]) REFERENCES [question_option] ([id]) ON DELETE NO ACTION
);
GO

-- ===================================================================
-- НАПОЛНЕНИЕ ТЕСТОВЫМИ ДАННЫМИ
-- Пароль для пользователей (SHA-1):
-- "admin123" -> F865B53623B121FD34EE5426C792E5C33AF8C227
-- "user123"  -> 38762CF7F55934B34D179AE6A4C80CADCCBB7F0A
-- ===================================================================

-- 1. Пользователи
INSERT INTO [user_account] ([login], [password_hash], [role], [full_name], [email], [phone], [photo])
VALUES
(N'admin', N'F865B53623B121FD34EE5426C792E5C33AF8C227', N'Admin', N'Корж Сергей Андреевич', N'korzh@edu.fa.ru', N'+7 (999) 111-22-33', N'/res/admin.png'),
(N'ivanov', N'38762CF7F55934B34D179AE6A4C80CADCCBB7F0A', N'User', N'Иванов Иван Иванович', N'ivanov@fa.ru', N'+7 (999) 222-33-44', N'/res/user1.png'),
(N'smirnova', N'38762CF7F55934B34D179AE6A4C80CADCCBB7F0A', N'User', N'Смирнова Анна Павловна', N'smirnova@fa.ru', N'+7 (999) 333-44-55', N'/res/user2.png'),
(N'kuznetsov', N'38762CF7F55934B34D179AE6A4C80CADCCBB7F0A', N'User', N'Кузнецов Артем Дмитриевич', N'kuznetsov@fa.ru', N'+7 (999) 444-55-66', NULL);
GO

-- 2. Категории
INSERT INTO [category] ([name])
VALUES
(N'Студенческая жизнь и колледж'),
(N'Информационные технологии'),
(N'Спорт и здоровый образ жизни'),
(N'Культура и мероприятия'),
(N'Оценка качества сервисов');
GO

-- 3. Опросы
INSERT INTO [poll] ([user_id], [category_id], [title], [description], [created_at], [status], [is_anonymous])
VALUES
(1, 1, N'Оценка организации учебной практики', N'Опрос направлен на выявление пожеланий студентов по проведению УП.', '2026-10-01T10:00:00', N'Активен', 0),
(1, 2, N'Выбор стека веб-разработки 2026', N'Голосование за приоритетные технологии для изучения.', '2026-10-03T12:30:00', N'Активен', 0),
(2, 3, N'Спортивные секции колледжа', N'Какие виды спорта вы хотите видеть в расписании?', '2026-10-05T14:15:00', N'Активен', 0),
(2, 1, N'Качество питания в столовой', N'Мониторинг качества обслуживания.', '2026-10-06T11:00:00', N'Активен', 1),
(3, 4, N'Тематика новогоднего студенческого вечера', N'Выбираем концепцию праздника.', '2026-10-08T16:20:00', N'Завершен', 0),
(3, 5, N'Удобство личного кабинета студента', N'Оценка обновленного интерфейса портала.', '2026-10-10T09:40:00', N'Активен', 0);
GO

-- 4. Вопросы
INSERT INTO [question] ([poll_id], [text], [type], [order_number])
VALUES
(1, N'Насколько понятны требования методических рекомендаций?', N'Одиночный выбор', 1),
(2, N'Какой фреймворк вы предпочитаете для Frontend?', N'Одиночный выбор', 1),
(3, N'Какую секцию вы бы посещали регулярнее?', N'Одиночный выбор', 1);
GO

-- 5. Варианты ответов
INSERT INTO [question_option] ([question_id], [text])
VALUES
(1, N'Полностью понятны'),
(1, N'Есть некоторые вопросы'),
(1, N'Требуется подробная консультация'),
(2, N'React / Next.js'),
(2, N'Vue.js / Nuxt'),
(2, N'WPF / Desktop C#'),
(3, N'Волейбол'),
(3, N'Баскетбол'),
(3, N'Настольный теннис');
GO

-- 6. Отклики (участие в опросах)
INSERT INTO [response] ([poll_id], [user_id], [created_at])
VALUES
(1, 2, '2026-10-02T11:00:00'),
(1, 3, '2026-10-02T12:00:00'),
(2, 2, '2026-10-04T15:30:00'),
(2, 3, '2026-10-04T16:45:00'),
(3, 3, '2026-10-07T10:10:00');
GO

-- 7. Ответы
INSERT INTO [answer] ([response_id], [question_id], [option_id], [text_answer])
VALUES
(1, 1, 1, NULL),
(2, 1, 1, NULL),
(3, 2, 6, NULL),
(4, 2, 4, NULL),
(5, 3, 7, NULL);
GO
