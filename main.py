# coding=utf-8
from config import Config, ConfigKeys
import slack_channel.slack_event_handler

if __name__ == '__main__':
    slack_channel.slack_event_handler.run_slack_socket_mode_client()
