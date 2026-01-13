# Step 4: Create the Remaining Agents

## Learning Objectives
1. Learn about the remaining question types (comparative, difference, intersection, multi-hop)
2. Learn how the classifier prompt identifies the remaining question types
3. Learn the steps an agent needs to do for the
- comparative agent
- difference agent
- intersection agent
- multi-hop agent

## Prerequisites
1. Must complete [step 3](step3.md) first

## Challenge
Now you have a good start knowing how the `yes_no_agent` and the `count_agent` are implemented and added to the workflow for the system to use when needed. It is now your turn to implement the additional agent types.

### Comparative Agent

#### Question Type Details
In the [Mintaka: A Complex, Natural, and Multilingual Dataset for End-to-End Question Answering](https://aclanthology.org/2022.coling-1.138.pdf) whitepaper:

> COMPARATIVE: questions that compare two objects on a given attribute (e.g., age, height). For example, Q: Is Mont Blanc taller than Mount Rainier? A: Yes

In our dataset some count questions could be:
- "Is consultant access more urgent than employee access issues?"
- "Are login failures more common than sync failures?"
- "Are cloud collaboration problems disrupting more teams than local issues?"
- "Are software development delays more impactful than HR process delays?"
- "Are third-party systems more problematic than internal systems?"

#### Classifier Prompt

The following is the portion of the classifier prompt important for the Comparative Agent:
```text
**COMPARATIVE_AGENT**: Questions comparing multiple items (more/less, vs, or)
   - **Keywords**: "more", "less", "vs", "versus", "or", "compared to", "better", "worse"
   - **Pattern**: "Do we have more [ITEM_A] or [ITEM_B]?" or "Which has more: [A] or [B]?"
   - Examples:
     - "Do we have more issues with MacBook Air computers or Dell XPS laptops?" ✓ COMPARATIVE_AGENT
     - "Which has more tickets: Surface Pro or iPad?" ✓ COMPARATIVE_AGENT
     - "Are there more incidents for HR or IT?" ✓ COMPARATIVE_AGENT
     - "Surface vs Dell: which has more problems?" ✓ COMPARATIVE_AGENT
```

### Steps the agent logic will need to take

#### Parse and Understand the Question
1. **Extract comparison items from the user's question** using an LLM
   - Send the question to an LLM with instructions to identify what's being compared
   - Parse the structured response to get item_1, item_2, and any additional items

#### Execute Searches
2. **Search for each item independently**
   - Loop through all comparison items
   - Run a search query for each item to get relevant tickets
   - Store the count and sample tickets for each item

#### Validate Results
3. **Check if data exists**
   - Verify that at least some tickets were found
   - Handle the case where no results exist for any item

#### Generate Comparison Analysis
4. **Build a summary of the comparison**
   - Create a count summary showing tickets per item
   - Format the detailed results for analysis
   - Structure a prompt that asks for comparative analysis

#### Return Formatted Results
5. **Prepare the final output**
   - Combine all data into an analysis-ready format
   - Include the counts, examples, and comparison context
   - Return a prompt that can generate the final comparative answer

#### Error Handling
6. **Handle edge cases gracefully**
   - Catch parsing errors when the LLM response isn't valid JSON
   - Provide meaningful error messages when things go wrong
   - Ensure the function always returns a usable response

This approach separates concerns cleanly: question understanding, data retrieval, and analysis preparation are distinct steps that can be tested and modified independently.

### Difference Agent

#### Question Type Details
In the [Mintaka: A Complex, Natural, and Multilingual Dataset for End-to-End Question Answering](https://aclanthology.org/2022.coling-1.138.pdf) whitepaper:

> DIFFERENCE: questions with a condition that contains a negation. For example, Q: Which Mario Kart game did Yoshi not appear in? A: Mario Kart Live: Home Circuit

In our dataset some count questions could be:
- "Which systems are consultants NOT able to access?"
- "Which Google Drive folders are NOT synchronizing?"
- "What login problems did NOT get fixed by the standard solution?"
- "Which browsers did NOT work for HR system access?"
- "Which solutions were NOT effective for login problems?"

#### Classifier Prompt

The following is the portion of the classifier prompt important for the Difference Agent:
```text
**DIFFERENCE_AGENT**: Questions asking for items that match one criterion but EXCLUDE/NOT another
   - **CRITICAL**: Look for negation words combined with "which", "what", "find", "show", "list"
   - **Negation indicators**: "not", "don't", "doesn't", "without", "excluding", "except", "no", "never"
   - **Exclusion phrases**: "does not mention", "does not involve", "don't mention", "don't involve", "not related to", "not about"
   - **Pattern**: [Which/What/Find] [TOPIC] [NEGATION] [EXCLUSION_TERM]
   - Examples:
     - "Which Dell XPS Issue does not mention Windows?" ✓ DIFFERENCE_AGENT
     - "What Surface problems don't involve the battery?" ✓ DIFFERENCE_AGENT
     - "Find tickets without high priority" ✓ DIFFERENCE_AGENT
     - "Issues that don't mention password" ✓ DIFFERENCE_AGENT
     - "Show me incidents not related to network" ✓ DIFFERENCE_AGENT
     - "Which problems exclude security?" ✓ DIFFERENCE_AGENT
```

### Steps the agent logic will need to take

#### Parse and Understand the Question
1. **Extract the main search and exclusion criteria** using an LLM
   - Send the question to an LLM to identify what to search for and what to exclude
   - Parse the structured response to get main_search and exclusion_term

#### Execute Two Searches
2. **Perform the main search**
   - Search for all tickets matching the primary criteria
   - Store the results as the base set

3. **Perform the exclusion search**
   - Search for tickets matching both the main criteria AND the exclusion term
   - These are the tickets to be removed from the final results

#### Calculate the Difference
4. **Find tickets in the first set but NOT in the second**
   - Extract IDs from the exclusion results
   - Filter main results to keep only those NOT in the exclusion set
   - This gives you the difference set

#### Validate Results
5. **Check for edge cases**
   - Handle when no main results are found
   - Handle when all results contain the exclusion term (empty difference)

#### Generate Difference Analysis
6. **Build a summary of the findings**
   - Show counts: total found, excluded, and remaining
   - Format the difference results for analysis
   - Structure a prompt that explains the difference logic

#### Return Formatted Results
7. **Prepare the final output**
   - Include the difference results with context
   - Provide counts and explanations
   - Return a prompt ready for final analysis generation

#### Error Handling
8. **Handle parsing and search failures**
   - Catch errors when the LLM response isn't valid JSON
   - Provide meaningful messages for empty result sets
   - Ensure the function always returns a usable response

This approach uses set difference logic: find everything matching criteria A, then subtract those that also match criteria B, leaving items that are A but NOT B.

### Intersection Agent

#### Question Type Details
In the [Mintaka: A Complex, Natural, and Multilingual Dataset for End-to-End Question Answering](https://aclanthology.org/2022.coling-1.138.pdf) whitepaper:

> INTERSECTION: questions that have two or more conditions that the answer must fulfill. For example, Q: Which movie was directed by Denis Villeneuve and stars Timothee Chalamet? A: Dune

In our dataset some count questions could be:
- "What issues affected HR team members AND involved Google Drive sync?"
- "What departments reported both sync failures AND access denials?"
- "Which requests are low priority AND related to consultant access?"
- "Which issues are both incidents AND requests?"
- "What browser settings cause issues AND affect external consultants?"

#### Classifier Prompt

The following is the portion of the classifier prompt important for the Intersection Agent:
```text
**INTERSECTION_AGENT**: Questions asking for items that match MULTIPLE criteria (AND logic)
   - **Only use when "and" combines SEARCH TOPICS, not database field filters**
   - Keywords: "and", "both", "that also", "with", "plus", "as well as"
   - Pattern: [What/Which/Find] [SEARCH_TOPIC_A] [AND] [SEARCH_TOPIC_B]
   - Examples:
     - "What issues are for Dell XPS laptops and the user tried Win + Ctrl + Shift + B?" ✓ INTERSECTION_AGENT (two search topics: "Dell XPS" AND "Win+Ctrl+Shift+B")
     - "Which Surface tickets involve battery problems and high priority?" ✗ NOT INTERSECTION - "high priority" is a Priority field filter
     - "Find incidents for HR and also mention password reset" ✓ INTERSECTION_AGENT (search for "HR incidents" AND "password reset")
     - "Show tickets with network issues that also have high priority" ✗ NOT INTERSECTION - "high priority" is a filter
```

### Steps the agent logic will need to take

#### Parse and Understand the Question
1. **Extract multiple search criteria** using an LLM
   - Send the question to an LLM to identify all criteria that must be matched
   - Parse the structured response to get criterion_1, criterion_2, and any additional criteria
   - Generate a combined search query

#### Execute Multiple Searches
2. **Perform a combined search**
   - Search using all criteria together in a single query
   - Store results as potential matches

3. **Perform individual searches for verification**
   - Search for each criterion separately to get broader result sets
   - These help verify true intersections

#### Calculate the Intersection
4. **Find items that match ALL criteria**
   - Get IDs from each individual search
   - Calculate set intersection (items appearing in ALL searches)
   - Filter combined results to include only true intersection items
   - Apply additional text-matching verification

#### Remove Duplicates
5. **Ensure unique results**
   - Track seen IDs to avoid duplicates
   - Build a clean list of unique intersection results

#### Validate Results
6. **Check for empty intersections**
   - Handle when no items match all criteria
   - Provide counts for each individual criterion
   - Explain why the intersection is empty

#### Generate Intersection Analysis
7. **Build a summary of the findings**
   - Show counts: individual matches and intersection size
   - Format the intersection results for analysis
   - Structure a prompt that explains what matches ALL criteria

#### Return Formatted Results
8. **Prepare the final output**
   - Include tickets that satisfy all conditions
   - Provide search statistics and context
   - Return a prompt ready for final analysis generation

#### Error Handling
9. **Handle parsing and search failures**
   - Catch errors when the LLM response isn't valid JSON
   - Provide meaningful messages for empty intersections
   - Ensure the function always returns a usable response

This approach uses set intersection logic: find items matching criterion A, items matching criterion B, then return only those that appear in BOTH sets (A ∩ B).

### Multi-hop Agent

#### Question Type Details
In the [Mintaka: A Complex, Natural, and Multilingual Dataset for End-to-End Question Answering](https://aclanthology.org/2022.coling-1.138.pdf) whitepaper:

> MULTI-HOP: questions that require 2 or more steps (multiple hops) to answer. For example, Q: Who was the quarterback of the team that won Super Bowl 50? A: Peyton Manning

In our dataset some count questions could be:
- "What was the final solution for the consultant who couldn't access the HR system for software development evaluation?"
- "How often do the authentication problems experienced by external consultants recur?"
- "Which follow-up actions were required after resolving the consultant's access problem?"
- "What specific HR module features are unavailable to the consultant with the reported access issue?"
- "Which department does the consultant work for who reported HR login failures affecting evaluation processes?"

#### Classifier Prompt

The following is the portion of the classifier prompt important for the Intersection Agent:
```text
**MULTI_HOP_AGENT**: Questions requiring multi-step reasoning (find X, then extract Y from X)
   - **Pattern**: "What [FIELD] had/has [CONDITION]?" or "Which [FIELD] [CONDITION]?"
   - **Indicators**: Questions asking for a different attribute than what's being searched
   - Examples:
     - "What department had consultants with Login Issues?" ✓ MULTI_HOP_AGENT (search for consultant login issues, extract department)
     - "Which priority level has the most printer problems?" ✓ MULTI_HOP_AGENT (search printer problems, extract priority)
     - "What ticket type do Surface issues get classified as?" ✓ MULTI_HOP_AGENT (search Surface issues, extract type)
     - "Which queue handles password reset requests?" ✓ MULTI_HOP_AGENT (search password reset, extract queue)
```

### Steps the agent logic will need to take

#### Parse and Understand the Question
1. **Extract the search query and target field** using an LLM
   - Send the question to an LLM to identify what to search for first
   - Identify what information needs to be extracted from those results
   - Capture the multi-hop reasoning logic

#### Execute the Initial Search
2. **Perform the first hop search**
   - Search for tickets matching the initial query
   - This gives you the base set of tickets to analyze

#### Validate Search Results
3. **Check if the first hop succeeded**
   - Verify that tickets were found
   - Handle the case where no initial results exist

#### Prepare for Information Extraction
4. **Structure the extraction task**
   - Format the search results for analysis
   - Clearly identify what field/information to extract
   - Set up the context for the second hop

#### Generate Multi-Hop Analysis
5. **Build the extraction prompt**
   - Include the original question and reasoning
   - Provide the search results from the first hop
   - Specify what information to extract (the second hop)
   - Request grouping and summarization by the target field

#### Return Formatted Results
6. **Prepare the final output**
   - Emphasize extraction of the target field
   - Include ticket citations as evidence
   - Return a prompt ready for final answer generation

#### Error Handling
7. **Handle parsing and search failures**
   - Catch errors when the LLM response isn't valid JSON
   - Provide meaningful messages when no results are found
   - Ensure the function always returns a usable response

This approach implements multi-hop reasoning: first find tickets matching criterion A, then extract information B from those tickets to answer what the user actually wants to know.

## [< Go to back to Step 3: Create the Count Search Agent](step3.md)