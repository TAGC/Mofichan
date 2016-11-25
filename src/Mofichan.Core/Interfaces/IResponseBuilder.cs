using System.Collections.Generic;

namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents a builder that facilitates constructing responses to other users.
    /// </summary>
    public interface IResponseBuilder
    {
        /// <summary>
        /// Provides the response builder with information about the message context
        /// which is necessary in order to produce certain types of responses.
        /// </summary>
        /// <param name="messageContext">The message context.</param>
        /// <returns>This builder.</returns>
        IResponseBuilder UsingContext(MessageContext messageContext);

        /// <summary>
        /// Specifies part of a response to be constructed from a raw string.
        /// </summary>
        /// <param name="rawString">A string to use in the response as-is.</param>
        /// <returns>This builder.</returns>
        IResponseBuilder FromRaw(string rawString);

        /// <summary>
        /// Specifies part of a response to be constructed using an article (snippet)
        /// chosen based on the specified tag requirements.
        /// </summary>
        /// <param name="tags">The tags to base the chosen article on.</param>
        /// <returns>This builder.</returns>
        /// <remarks>
        /// The set of tags is passed as a double-nested enumeration. The outer enumeration
        /// specifies a requirement for <i>any</i> of the inner enumeration elements to be
        /// satisfied, while each inner enumeration specifies a requirement for <i>all</i>
        /// of its constituent tag requirements to be satisfied.
        /// </remarks>
        IResponseBuilder FromTags(IEnumerable<IEnumerable<Tag>> tags);

        /// <summary>
        /// Specifies part of a response to be constructed using an article (snippet)
        /// chosen based on the specified tag requirements.
        /// </summary>
        /// <param name="chance">
        /// A value between 0 and 1 representing the chance that an article is selected.
        /// <para></para>
        /// A random value following a uniform distribution will be sampled to determine
        /// if an article is included or not.
        /// </param>
        /// <param name="tags">The tags to base the chosen article on.</param>
        /// <returns>
        /// This builder.
        /// </returns>
        /// <remarks>
        /// The set of tags is passed as a double-nested enumeration. The outer enumeration
        /// specifies a requirement for <i>any</i> of the inner enumeration elements to be
        /// satisfied, while each inner enumeration specifies a requirement for <i>all</i>
        /// of its constituent tag requirements to be satisfied.
        /// </remarks>
        IResponseBuilder FromTags(double chance, IEnumerable<IEnumerable<Tag>> tags);

        /// <summary>
        /// Specifies part of a response to be constructed using an article (snippet)
        /// chosen based on the specified tag requirements.
        /// </summary>
        /// <param name="prefix">A string to prepend to a chosen article.</param>
        /// <param name="tags">The tags to base the chosen article on.</param>
        /// <returns>
        /// This builder.
        /// </returns>
        /// <remarks>
        /// The set of tags is passed as a double-nested enumeration. The outer enumeration
        /// specifies a requirement for <i>any</i> of the inner enumeration elements to be
        /// satisfied, while each inner enumeration specifies a requirement for <i>all</i>
        /// of its constituent tag requirements to be satisfied.
        /// </remarks>
        IResponseBuilder FromTags(string prefix, IEnumerable<IEnumerable<Tag>> tags);

        /// <summary>
        /// Specifies part of a response to be constructed using an article (snippet)
        /// chosen based on the specified tag requirements.
        /// </summary>
        /// <param name="prefix">
        /// A string to prepend to a chosen article.
        /// <para></para>
        /// If no article is selected, the prefix will be omitted from the response too.
        /// </param>
        /// <param name="chance">
        /// A value between 0 and 1 representing the chance that an article is selected.
        /// <para></para>
        /// A random value following a uniform distribution will be sampled to determine
        /// if an article is included or not.
        /// </param>
        /// <param name="tags">The tags to base the chosen article on.</param>
        /// <returns>
        /// This builder.
        /// </returns>
        /// <remarks>
        /// The set of tags is passed as a double-nested enumeration. The outer enumeration
        /// specifies a requirement for <i>any</i> of the inner enumeration elements to be
        /// satisfied, while each inner enumeration specifies a requirement for <i>all</i>
        /// of its constituent tag requirements to be satisfied.
        /// </remarks>
        IResponseBuilder FromTags(string prefix, double chance, IEnumerable<IEnumerable<Tag>> tags);

        /// <summary>
        /// Specifies part of a response to be constructed using one of a provided
        /// set of phrases.
        /// </summary>
        /// <param name="phrases">The phrases to choose from.</param>
        /// <returns>This builder.</returns>
        IResponseBuilder FromAnyOf(params string[] phrases);

        /// <summary>
        /// Specifies part of a response to be constructed using one of a provided
        /// set of phrases.
        /// </summary>
        /// <param name="chance">
        /// A value between 0 and 1 representing the chance that a phrase is selected.
        /// <para></para>
        /// A random value following a uniform distribution will be sampled to determine
        /// if a phrase is included or not.
        /// </param>
        /// <param name="phrases">The phrases to choose from.</param>
        /// <returns>
        /// This builder.
        /// </returns>
        IResponseBuilder FromAnyOf(double chance, IEnumerable<string> phrases);

        /// <summary>
        /// Specifies part of a response to be constructed using one of a provided
        /// set of phrases.
        /// </summary>
        /// <param name="prefix">A string to prepend to a chosen phrase.</param>
        /// <param name="phrases">The phrases to choose from.</param>
        /// <returns>
        /// This builder.
        /// </returns>
        IResponseBuilder FromAnyOf(string prefix, IEnumerable<string> phrases);

        /// <summary>
        /// Specifies part of a response to be constructed using one of a provided
        /// set of phrases.
        /// </summary>
        /// <param name="prefix">
        /// A string to prepend to a chosen phrase.
        /// <para></para>
        /// If no phrase is selected, the prefix will be omitted from the response too.
        /// </param>
        /// <param name="chance">
        /// A value between 0 and 1 representing the chance that a phrase is selected.
        /// <para></para>
        /// A random value following a uniform distribution will be sampled to determine
        /// if a phrase is included or not.
        /// </param>
        /// <param name="phrases">The phrases to choose from.</param>
        /// <returns>This builder.</returns>
        IResponseBuilder FromAnyOf(string prefix, double chance, IEnumerable<string> phrases);

        /// <summary>
        /// Constructs a response based on the configured state of this builder.
        /// </summary>
        /// <returns>A string response.</returns>
        string Build();
    }
}
