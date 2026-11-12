# LiteTelegramTrashSorter

Telegram userbot that automatically filters posts by parameters from subscribed channels using AI

## Features

- Scrapes recent messages from all subscribed channels (groups != channels)
- Uses DeepSeek AI to analyze and filter posts by prompt (prompt in LTGTrashSorter.Worker\appsettings.json)
- Automatically forwards significant news to a target channel
- SQLite to save processing state and avoid duplicate analysis after restarts

## Setup

1. Get your `api_id` and `api_hash` from [my.telegram.org](https://my.telegram.org/).
2. Fill credentials and AI keys in `.env` (priority) or `appsettings.json`. (Create `.env` in the root if needed)
3. Run interactively to authorize and create session/database files:
   ```bash
   docker compose run --rm ltgtrashsorter
   # Wait until db initializing 
   # Wait until tg asks for a code (Check chat with tg on phone/pc)
   # After first init ctrl + c for exit
   ```
4. Verify that `database.db` and `WTelegram.session` created in project root `./data/` (can be changed in compose.yaml) folder.
> if files doesn't created check folder rules
5. Run the service normally in the background
   ```bash
   docker compose up -d --build
   ```

## .env example
```
# App for userbot creditails
Settings__Telegram__ApiId=
Settings__Telegram__ApiHash=

# Account creditails
Settings__Telegram__PhoneNumber=
Settings__Telegram__Password=

# Channels configuration
Settings__Telegram__TargetChannelId=
Settings__Telegram__LogChannelId=

# Storage and Session paths
Settings__Telegram__SessionPath=/app/data/WTelegram.session
Settings__DB__DBPath=/app/data/database.db

# Keys
Settings__Ai__ApiKey=

# Pre post forward / branding
Settings__Telegram__TargetChannelLinkText= 
Settings__Telegram__TargetChannelLinkUrl=
```

## Project example
- [OnlyNews](https://t.me/OnlyNewsAi) - noises and clickbait free news aggregator 

## PS
**Use it responsibly and in accordance with Telegram's rules.**