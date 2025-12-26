#!/bin/sh

# Important:
# - If you "source" this script, it runs in your current shell.
# - Calling "exit" in that case will close your shell (and your terminal tab).
# This script therefore returns when sourced, and exits when executed.

print_usage() {
	cat <<'USAGE'
Usage:
	source ./setup-gh-auth.sh --username <value> --token <value>
	source ./setup-gh-auth.sh -u <value> -t <value>

Options:
	-u, --username   GitHub Packages username (e.g., your GitHub handle)
	-t, --token      GitHub Packages token (PAT) with appropriate scopes
	-h, --help       Show this help

Notes:
	- To persist exported variables in your current shell, you must source this script.
		If you run it ("./scripts/setup-gh-auth.sh"), exports only apply to the subprocess.
USAGE
}

is_sourced=0
if [[ -n "${BASH_VERSION-}" ]]; then
	# shellcheck disable=SC2128
	if [[ "${BASH_SOURCE[0]}" != "$0" ]]; then
		is_sourced=1
	fi
elif [[ -n "${ZSH_VERSION-}" ]]; then
	case "${ZSH_EVAL_CONTEXT-}" in *:file) is_sourced=1 ;; esac
fi

username=""
token=""

error_msg=""
show_help=0

while [[ $# -gt 0 ]]; do
	case "$1" in
		-u|--username)
			if [[ $# -lt 2 ]]; then
				error_msg="Missing value for $1"
				break
			fi
			username="$2"
			shift 2
			;;
		-t|--token)
			if [[ $# -lt 2 ]]; then
				error_msg="Missing value for $1"
				break
			fi
			token="$2"
			shift 2
			;;
		-h|--help)
			show_help=1
			shift
			;;
		*)
			error_msg="Unknown argument: $1"
			break
			;;
	esac
done

if [[ "$show_help" -eq 1 ]]; then
	print_usage
	if [[ "$is_sourced" -eq 1 ]]; then
		return 0
	fi
	exit 0
fi

if [[ -n "$error_msg" ]]; then
	echo "Error: $error_msg" >&2
	echo >&2
	print_usage >&2
	if [[ "$is_sourced" -eq 1 ]]; then
		return 1
	fi
	exit 1
fi

if [[ -z "$username" ]]; then
	echo "Error: --username/-u is required" >&2
	echo >&2
	print_usage >&2
	if [[ "$is_sourced" -eq 1 ]]; then
		return 1
	fi
	exit 1
fi

if [[ -z "$token" ]]; then
	echo "Error: --token/-t is required" >&2
	echo >&2
	print_usage >&2
	if [[ "$is_sourced" -eq 1 ]]; then
		return 1
	fi
	exit 1
fi

export GH_PACKAGE_USERNAME="$username"
export GH_PACKAGE_TOKEN="$token"

echo "Exported GH_PACKAGE_USERNAME and GH_PACKAGE_TOKEN for this shell session."
