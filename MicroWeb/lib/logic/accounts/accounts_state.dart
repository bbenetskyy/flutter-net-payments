part of 'accounts_bloc.dart';

class AccountsState extends Equatable {
  const AccountsState({
    this.loading = false,
    this.items = const [],
    this.error,
    this.submitting = false,
    this.submitError,
  });

  const AccountsState.initial() : this();

  final bool loading;
  final List<AccountResponse> items;
  final String? error;

  final bool submitting;
  final String? submitError;

  AccountsState copyWith({
    bool? loading,
    List<AccountResponse>? items,
    String? error,
    bool? submitting,
    String? submitError,
  }) {
    return AccountsState(
      loading: loading ?? this.loading,
      items: items ?? this.items,
      error: error,
      submitting: submitting ?? this.submitting,
      submitError: submitError,
    );
  }

  @override
  List<Object?> get props => [loading, items, error, submitting, submitError];
}
