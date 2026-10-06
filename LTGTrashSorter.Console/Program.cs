// See https://aka.ms/new-console-template for more information

using LTGTrashSorter.AI.Services;

IAiService aiSerive = new DeepSeekService("");

string systemPrompt = """
                      Ты — строгий классификатор рабочих сообщений из Telegram для системы умного будильника.
                      Твоя задача — оценить важность сообщения для дежурного сотрудника.
                      
                      Ответь СТРОГО в формате JSON без кавычек кода и пояснений:
                      {
                        "isUrgent": true/false,
                        "reason": "краткая причина",
                        "id": int
                      }

                      Правила:
                      - isUrgent = true ТОЛЬКО если зовут по имени (Имя/Ник), упал сервер, авария, или просят срочно включиться.
                      - Fluff, приветствия, мемы, обычные обсуждения — isUrgent = false.
                      - id - id сообщения триггера, если алёрт комплексны - пиши id первого сообщения с котором начинается алерт
                      """;

string message = "";

var result = await aiSerive.ProcessMessageAsync(systemPrompt,message);
Console.WriteLine(result);