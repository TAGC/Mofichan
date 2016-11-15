Mofichan
========

.. image:: https://ci.appveyor.com/api/projects/status/0lnl92u04uxwtpyp/branch/develop?svg=true
   :target: https://ci.appveyor.com/project/TAGC/mofichan/branch/develop
   :alt: AppVeyor Build Status

Mofichan is a chatbot under recent development.

Unlike other chatbots, the goal isn't to just provide her with utility functions but to give her a personality as well. Currently, Mofichan only works on Discord.

Architecture
------------

Mofichan is designed with a modular architecture. This makes it simpler to extend her to incorporate new behaviours and operate on different platforms.

Her *backend architecture* is based around the `template method pattern <https://sourcemaking.com/design_patterns/template_method>`_; a general template is provided for joining rooms, leaving rooms, sending messages, etc. and backends specialised for a particular platform (like Discord) fill in the blanks.

Her *behaviour architecture* is based around the `chain of responsibility pattern <https://sourcemaking.com/design_patterns/chain_of_responsibility>`_, in which different aspects of her behaviour are segregated into different "behaviour modules" that can be chained together at runtime and operate independently of one another. When Mofichan receives messages, they pass through the behaviour chain and each behaviour is given a chance to perform processing on it, form a response or skip the remainder of the chain.

Behaviour modules can also perform "background" logic to allow Mofichan to perform actions independent of arriving messages.
