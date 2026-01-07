# Instructors for Instructors

## To prep this tree

- from `.../labs/python` run `python -m venv .venv`
- create `requirements.txt` containing all the required libraries for ALL labs combined

## Adding a lab

Be sure to add any new dependency requirements to `.../labs/python/requirements.txt`

Consider `pip freeze > requirements.txt` or `pip freeze >> requirements.txt` as useful.

## Authentication

If `az login` is not the approach, you can test with `az account show` (after `az logout` if necessary) to be sure you are really testing the non-authenticated path.
