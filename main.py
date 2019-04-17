# coding=utf-8
# from slack_channel.initial_memberlist_populator import InitialMemberlistPopulator
from slack_channel.slack_event_handler import SlackEventHandler

if __name__ == '__main__':
    # InitialMemberlistPopulator().populate()
    handler = SlackEventHandler(True)
    handler.handle_slack_context()
