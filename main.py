# coding=utf-8
from config import Config, ConfigKeys
from slack_channel.slack_event_handler import SlackEventHandler

if __name__ == '__main__':
    config = Config()
    env_mode = config.get_config_value(ConfigKeys.env_key)
    is_debug = env_mode != "live"
    handler = SlackEventHandler(is_debug)
    handler.handle_slack_context()
