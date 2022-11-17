import logging
from model.karma import KarmaType

from commands import AddKarmaCommand
from slack_channel.abstract_reaction_handler import AbstractReactionHandler

class AddKarmaReactionHandler(AbstractReactionHandler):

    @property
    def command(self) -> AddKarmaCommand:
        return AddKarmaCommand()
    
    def can_handle(self, reaction_add_event) -> bool:
        reaction = self.parse_reaction(reaction_add_event["reaction"])
        return reaction in AddKarmaCommand.get_karma_reactions()
    
    def _invoke_handler_logic(self, reaction_add_event):
        reactions = AddKarmaCommand.get_karma_reactions()
        try:
            reaction = self.parse_reaction(reaction_add_event["reaction"])
            self.command.execute(awarded_to=reaction_add_event["reactee"],
                                 awarded_by=reaction_add_event["reactor"],
                                 reason="Reacted with " + reaction,
                                 karma_type=reactions[reaction])
            self._send_reaction_response(reaction_add_event)
        except Exception as ex:
            logging.exception(ex)

    def parse_reaction(self, raw_reaction):
        return raw_reaction.split('::')[0]
