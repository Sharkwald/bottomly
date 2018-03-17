# coding=utf-8
from slacksocket import SlackSocket
from commands.google_search import GoogleSearchCommand
from config import Config, ConfigKeys


config = Config()
token = config.get_config_value(ConfigKeys.slack_bot_token)


def _is_google_command(slack_event):
    text = slack_event['text']
    return text.startswith("_g ")


def _send_google_response(response_message, slack_event):
    with SlackSocket(token) as s:
        msg = s.send_msg(response_message, slack_event['channel'])
        print(msg.sent)


def _is_subscribed_event(slack_event):
    try:
        subscribed = True
        subscribed = subscribed and slack_event['type'] == "message"
        subscribed = subscribed and "text" in slack_event
        return subscribed
    except Exception as ex:
        print("Error determining if event is subscribed: " + str(ex))
        print("Message: " + slack_event)



class SlackEventHandler(object):
    def handle_slack_context(self):
        print("opening web socket to slack...")
        with SlackSocket(token) as s:
            try:
                for e in s.events():
                    print(e.json)
                    slack_event = e.event
                    if not _is_subscribed_event(slack_event):
                        continue
                    if _is_google_command(slack_event):
                        q = slack_event['text'][3:]
                        c = GoogleSearchCommand()
                        result = c.execute(q)
                        response_message = result['title'] + " " + result["link"]
                        if self.debug:
                            response_message = "[DEBUG] " + response_message
                        _send_google_response(response_message, slack_event)
            except Exception as ex:
                print("Error! " + str(ex))

    def __init__(self, debug=False):
        self.debug = debug
        super(SlackEventHandler, self).__init__()
