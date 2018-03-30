import datetime
from googleapiclient.discovery import build

from commands.abstract_command import AbstractCommand
from config import Config, ConfigKeys


class GetNextCalendarEventCommand(AbstractCommand):

    def get_purpose(self):
        return "Gets the next event in a specified Google calendar."

    def execute(self, calendar_id):
        service = build('calendar', 'v3', developerKey=self.api_key)

        now = datetime.datetime.utcnow().isoformat() + 'Z'  # 'Z' indicates UTC time
        print('Getting the upcoming 10 events')
        events_result = service.events().list(
            calendarId=calendar_id,
            timeMin=now, maxResults=1, singleEvents=True,
            orderBy='startTime').execute()
        events = events_result.get('items', [])
        return events[0]

    def __init__(self):
        super(GetNextCalendarEventCommand, self)
        config = Config()
        self.api_key = config.get_config_value(ConfigKeys.google_api_key)
