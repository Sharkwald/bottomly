# coding=utf-8
import datetime
import logging
import logging.config

from slack_sdk import WebClient
from slack_sdk.rtm import RTMClient
from slack_sdk.errors import SlackApiError

from config import Config, ConfigKeys
from model.member import Member
from slack_channel import *

live_mode = "live"

config = Config()
token = config.get_config_value(ConfigKeys.slack_bot_token)
debug = Config().get_config_value(ConfigKeys.env_key) != live_mode

logging.config.fileConfig('logging.conf')
logger = logging.getLogger('bottomly')
for handler in logger.handlers:
    if debug:
        handler.setLevel(logging.INFO)
    else:
        handler.setLevel(logging.WARN)

_command_handlers = list([
    GoogleEventHandler(debug),
    UrbanEventHandler(debug),
    WikipediaEventHandler(debug),
    IncrementKarmaEventHandler(debug),
    DecrementKarmaEventHandler(debug),
    GetCurrentNetKarmaEventHandler(debug),
    GetCurrentKarmaReasonsEventHandler(debug),
    GiphyEventHandler(debug),
    GetLoserBoardEventHandler(debug),
    GetLeaderBoardEventHandler(debug),
    RegEventHandler(debug),
    TestEventHandler(debug),
    ReactionTestHandler(debug),
    GoogleImageEventHandler(debug),
    # CocktailOfTheWeekEventHandler(debug),
    ReleaseEventHandler(debug)
])

_reaction_handlers = list([
    AddKarmaReactionHandler(debug)
])

_channel_list = WebClient(token=token).conversations_list().data.get("channels")


def _is_subscribed_message(slack_event):
    try:
        subscribed = True
        subscribed = subscribed and "text" in slack_event
        subscribed = subscribed and "bot_id" not in slack_event
        return subscribed
    except Exception as ex:
        logger.warning("Error determining if message is subscribed: " + str(ex))
        logger.warning("Message: " + str(slack_event))
    return False


def _is_subscribed_reaction(slack_event):
    try:
        subscribed = True
        # subscribed = subscribed and "bot_id" not in slack_event
        return subscribed
    except Exception as ex:
        logger.warning("Error determining if reaction is subscribed: " + str(ex))
        logger.warning("Reaction Event: " + str(slack_event))
    return False


def handle_slack_context():
    try:
        rtm_client = RTMClient(token=token)
        logger.info("Opening connection to slack.")
        rtm_client.start()
    except SlackApiError:
        logger.exception("Error establishing connection to slack.")


@RTMClient.run_on(event="message")
def _process_slack_message(**e):
    try:
        slack_event = e["data"]
        if not _is_subscribed_message(slack_event):
            return

        logger.info("Subscribing to: " + str(slack_event))

        _insert_channel_id_to_message(slack_event)
        _insert_username(slack_event)

        handled = False

        help_handler = HelpEventHandler(debug, _command_handlers)
        if help_handler.can_handle(slack_event):
            handled = help_handler.handle(slack_event)

        if handled:
            return

        handled = _execute_command_handlers(slack_event)

        if handled:
            return

        # TODO: Dynamic response handler

    except SlackApiError:
        logger.exception("Error processing slack message")


@RTMClient.run_on(event="reaction_added")
def _process_slack_reaction(**e):
    try:
        slack_event = e["data"]
        if not _is_subscribed_reaction(slack_event):
            return
        slack_event["channel"] = slack_event["item"]["channel"]

        slack_event["reactor"] = Member.get_member_by_slack_id(slack_event["user"]).username
        slack_event["reactee"] = Member.get_member_by_slack_id(slack_event["item_user"]).username

        logger.info(str(slack_event))

        _reaction_handlers[0].handle(slack_event)

    except SlackApiError:
        logger.exception("Slack API Error processing slack reaction")
    
    except Exception as ex:
        logger.exception("General error processing slack reaction: " + str(ex))


def _execute_command_handlers(slack_event):
    handled = False
    for handler in _command_handlers:
        if handler.can_handle(slack_event):
            handler.handle(slack_event)
            handled = True
            continue
    return handled


def _insert_channel_id_to_message(message_event):
    try:
        known_channel_names = [c["name"] for c in _channel_list]
        if message_event["channel"] in known_channel_names:
            channel = [c for c in _channel_list if c["name"] == message_event["channel"]][0]
            message_event["channel_id"] = channel["id"]
    except Exception:
        logger.exception("Error loading channel id from message: " + str(message_event))


def _insert_username(slack_event):
    try:
        member = Member.get_member_by_slack_id(slack_event["user"])
        slack_event["user_id"] = slack_event["user"]
        slack_event["user"] = member.username

    except Exception:
        logger.exception("Error loading user id from event: " + str(slack_event))
