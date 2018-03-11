from datetime import datetime, timedelta

class Member(object):
	"""Represents a member of a chat channel"""

	def get_karma():
		"""a function doc string"""
		cut_off = date.today() - timedelta(days=30);
		return filter((lambda k: k.awarded < cut_off), __karmas)

	def __init__(self, karmas):
		super(Member, self).__init__()
		self.__karmas = karmas
		
class Karma(object):
	"""docstring for Karma"""
	def __init__(self, awarder, reason, awarded):
		super(Karma, self).__init__()
		self.awarder = awarder
		self.reason = reason
		self.awarded = awarded

		