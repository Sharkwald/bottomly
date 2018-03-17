# coding=utf-8
from slacksocket import SlackSocket
from commands.google_search import GoogleSearchCommand
from config import Config, ConfigKeys


config = Config()
token = config.get_config_value(ConfigKeys.slack_bot_token)

class SlackEventHandler(object):
    def handle_slack_context(self):
        print("opening web socket to slack...")
        with SlackSocket(token) as s:
            try:
                for event in s.events():
                    print(event.json)
                    if (self._is_google_command(event.event)):
                        q = event.event['text'][3:]
                        c = GoogleSearchCommand()
                        result = c.execute(q)
                        response_message = result['title'] + " " + result["link"]
                        if self.debug:
                            response_message = "[DEBUG] " + response_message
                        self.send_google_response(response_message, event.event)
            except Exception as ex:
                print("Error! " + str(ex))

    def _is_google_command(self, slack_event):
        if slack_event['type'] != "message":
            return False
        if not 'text' in slack_event:
            return False
        text = slack_event['text']
        return text.startswith("_g ")

    def send_google_response(self, response_message, slack_event):
        with SlackSocket(token) as s:
            msg = s.send_msg(response_message, slack_event['channel'])
            print(msg.sent)


    def __init__(self, debug=False):
        self.debug = debug
        super(SlackEventHandler, self).__init__()