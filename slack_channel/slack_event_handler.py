# coding=utf-8
from slacksocket import SlackSocket
from config import Config, ConfigKeys
from slack_channel import *

config = Config()
token = config.get_config_value(ConfigKeys.slack_bot_token)


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
    def _init_handlers(self):
        self.handlers = list([
            GoogleEventHandler(self.debug),
            UrbanEventHandler(self.debug),
            WikipediaEventHandler(self.debug)
        ])

    def handle_slack_context(self):
        if self.debug:
            print("opening web socket to slack...")

        with SlackSocket(token) as s:
            try:
                for e in s.events():
                    if self.debug:
                        print(e.json)

                    slack_event = e.event
                    if not _is_subscribed_event(slack_event):
                        continue
                    for handler in self.handlers:
                        if handler.can_handle(slack_event):
                            handler.handle(slack_event)
                            continue

            except Exception as ex:
                print("Error! " + str(ex))
                # TODO: Log this I guess?

    def __init__(self, debug=False):
        self.debug = debug
        self._init_handlers()
        super(SlackEventHandler, self).__init__()
