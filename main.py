# coding=utf-8
from slacksocket import SlackSocket

from commands.google_search import GoogleSearchCommand
from config import Config, ConfigKeys

config = Config()
token = config.get_config_value(ConfigKeys.slack_bot_token)

def do_it():
    with SlackSocket(token) as s:
        try:
            for event in s.events():
                print(event.json)
                if (is_google_command(event.event)):
                    q = event.event['text'][3:]
                    c = GoogleSearchCommand()
                    result = c.execute(q)
                    response_message = result['title'] + " " + result["link"]
                    send_google_response(response_message, event.event)
        except Exception as ex:
            print("Error! " + str(ex))


def is_google_command(slack_event):
    if slack_event['type'] != "message":
        return False
    if not 'text' in slack_event:
        return False
    text = slack_event['text']
    return text.startswith("_g ")

def send_google_response(response_message, slack_event):
    with SlackSocket(token) as s:
        msg = s.send_msg(response_message, slack_event['channel'])
        print(msg.sent)

if __name__ == '__main__':
    do_it()
