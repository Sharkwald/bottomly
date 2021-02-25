# coding=utf-8
from config import Config, ConfigKeys
import slack_channel.slack_event_handler

live_mode = "live"

if __name__ == '__main__':
    is_debug = Config().get_config_value(ConfigKeys.env_key) != live_mode
    slack_channel.slack_event_handler.handle_slack_context()
