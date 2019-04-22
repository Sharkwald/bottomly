# coding=utf-8
from config import Config, ConfigKeys
from slack_channel.slack_event_handler import SlackEventHandler

live_mode = "live"

if __name__ == '__main__':
    is_debug = Config().get_config_value(ConfigKeys.env_key) != live_mode
    handler = SlackEventHandler(is_debug)
    handler.handle_slack_context()
