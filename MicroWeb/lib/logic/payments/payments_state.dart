import 'package:equatable/equatable.dart';

import '../../data/models/responses/account_response.dart';
import '../../data/models/responses/payment_response.dart';
import '../../data/models/responses/user_response.dart';

class PaymentsState extends Equatable {
  const PaymentsState({
    this.loading = false,
    this.items = const [],
    this.error,
    this.submitting = false,
    this.submitError,
    this.currentUser,
    this.myAccounts = const [],
    this.users = const [],
    this.beneficiaryAccounts = const [],
  });

  final bool loading;
  final List<PaymentResponse> items;
  final String? error;

  final bool submitting;
  final String? submitError;

  final UserResponse? currentUser;
  final List<AccountResponse> myAccounts;
  final List<UserResponse> users;
  final List<AccountResponse> beneficiaryAccounts;

  PaymentsState copyWith({
    bool? loading,
    List<PaymentResponse>? items,
    String? error,
    bool? submitting,
    String? submitError,
    UserResponse? currentUser,
    List<AccountResponse>? myAccounts,
    List<UserResponse>? users,
    List<AccountResponse>? beneficiaryAccounts,
  }) {
    return PaymentsState(
      loading: loading ?? this.loading,
      items: items ?? this.items,
      error: error,
      submitting: submitting ?? this.submitting,
      submitError: submitError,
      currentUser: currentUser ?? this.currentUser,
      myAccounts: myAccounts ?? this.myAccounts,
      users: users ?? this.users,
      beneficiaryAccounts: beneficiaryAccounts ?? this.beneficiaryAccounts,
    );
  }

  @override
  List<Object?> get props => [
    loading,
    items,
    error,
    submitting,
    submitError,
    currentUser,
    myAccounts,
    users,
    beneficiaryAccounts,
  ];
}
