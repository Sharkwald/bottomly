
import unittest
from unittest.mock import patch

import os

from commands import AddKarmaCommand
from config import Config
from slack_channel import IncrementKarmaEventHandler

class TestIncrementKarmaHandler(unittest.TestCase):
    