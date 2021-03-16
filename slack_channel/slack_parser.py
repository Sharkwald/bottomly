from model.member import Member

def _is_slack_id(word):
    return word.startswith('<@') and word.endswith('>')

def _strip_slack_token_formatting(word):
    return word[2:len(word) - 1]

def _convert_slack_id_to_username(slack_id):
    m = Member.get_member_by_slack_id(slack_id)
    if m is not None:
        return m.username
    raise Exception("User not found")


class SlackParser(object):

    @staticmethod
    def replace_slack_id_tokens_with_usernames(slack_message):
        words = slack_message.split(' ')
        new_words = list([])
        for word in words:
            new_word = word
            if _is_slack_id(word):
                new_word = _strip_slack_token_formatting(word)
                new_word = _convert_slack_id_to_username(new_word)
            new_words.append(new_word)

        if slack_message.find(' ') != -1:
            updated_message = ' '.join(new_words)
        else:
            updated_message = new_words[0]

        return updated_message