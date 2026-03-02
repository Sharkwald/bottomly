# coding=utf-8
from config import Config, ConfigKeys
import slack_channel.slack_event_handler

live_mode = "live"

if __name__ == '__main__':
    is_debug = Config().get_config_value(ConfigKeys.env_key) != live_mode
    connection_mode = Config().get_config_value(ConfigKeys.slack_connection_mode)
    # if connection_mode == "RTM":
    #     slack_channel.slack_event_handler.run_slack_rtm_client()
    # else:
    slack_channel.slack_event_handler.run_slack_socket_mode_client()
