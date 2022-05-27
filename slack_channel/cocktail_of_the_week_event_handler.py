# coding=utf-8
from commands import CocktailOfTheWeekSearchCommand
from slack_channel.abstract_event_handler import AbstractEventHandler


command_symbol = "cotw"
empty_result_message = "You're on your own I'm afraid - https://thebrassmonkeygla.co.uk/."


class CocktailOfTheWeekEventHandler(AbstractEventHandler):
    @property
    def command(self) -> CocktailOfTheWeekSearchCommand:
        return CocktailOfTheWeekSearchCommand()

    @property
    def name(self):
        return "Cocktail of the week lookup from the Brass Monkey"

    def get_usage(self):
        return self.command_trigger

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger)

    def _invoke_handler_logic(self, slack_event):
        response_message = self.command.execute()
        if response_message is None:
            self._send_message_response(empty_result_message, slack_event)
        else:
            self._send_message_response(response_message, slack_event)

    def _get_command_symbol(self):
        return command_symbol
