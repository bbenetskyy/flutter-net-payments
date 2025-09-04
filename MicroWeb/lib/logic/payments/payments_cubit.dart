import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../data/models/requests/create_payment_request.dart';
import '../../data/models/responses/payment_response.dart';
import '../../data/models/responses/user_response.dart';
import '../../data/repositories/payments_repository.dart';
import '../../data/repositories/users_repository.dart';

class PaymentsState extends Equatable {
  const PaymentsState({
    this.loading = false,
    this.items = const [],
    this.error,
    this.submitting = false,
    this.submitError,
    this.currentUser,
  });

  final bool loading;
  final List<PaymentResponse> items;
  final String? error;

  final bool submitting;
  final String? submitError;

  final UserResponse? currentUser;

  PaymentsState copyWith({
    bool? loading,
    List<PaymentResponse>? items,
    String? error,
    bool? submitting,
    String? submitError,
    UserResponse? currentUser,
  }) {
    return PaymentsState(
      loading: loading ?? this.loading,
      items: items ?? this.items,
      error: error,
      submitting: submitting ?? this.submitting,
      submitError: submitError,
      currentUser: currentUser ?? this.currentUser,
    );
  }

  @override
  List<Object?> get props => [loading, items, error, submitting, submitError, currentUser];
}

class PaymentsCubit extends Cubit<PaymentsState> {
  PaymentsCubit(this._repo, this._usersRepo) : super(const PaymentsState());

  final PaymentsRepository _repo;
  final UsersRepository _usersRepo;

  Future<void> _loadUser() async {
    try {
      // As requested, call GET '/users' which returns user_response
      final data = await _usersRepo.listUsers();
      final list = UserResponse.listFromJson(data);
      final me = list.isNotEmpty ? list.first : null;
      if (me != null) {
        emit(state.copyWith(currentUser: me));
      }
    } catch (_) {
      // Ignore errors for user prefetch; payments flow should still proceed
    }
  }

  Future<void> load({Map<String, dynamic>? query}) async {
    emit(state.copyWith(loading: true, error: null));
    await _loadUser();
    try {
      final data = await _repo.listPayments(query: query);
      final list = PaymentResponse.listFromJson(data);
      emit(state.copyWith(loading: false, items: list, error: null));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
    }
  }

  Future<PaymentResponse?> loadOne(String id) async {
    await _loadUser();
    try {
      final data = await _repo.getPaymentById(id);
      final list = PaymentResponse.listFromJson(data);
      final p = list.isNotEmpty ? list.first : null;
      if (p != null) {
        final updated = [...state.items];
        final idx = updated.indexWhere((e) => e.id == p.id);
        if (idx == -1) {
          updated.insert(0, p);
        } else {
          updated[idx] = p;
        }
        emit(state.copyWith(items: updated));
      }
      return p;
    } catch (_) {
      return null;
    }
  }

  Future<PaymentResponse?> create(CreatePaymentRequest request) async {
    emit(state.copyWith(submitting: true, submitError: null));
    await _loadUser();
    try {
      final data = await _repo.createPayment(request);
      final list = PaymentResponse.listFromJson(data);
      final created = list.isNotEmpty ? list.first : null;
      if (created != null) {
        final updated = [created, ...state.items];
        emit(state.copyWith(submitting: false, items: updated));
      } else {
        // if backend doesn't return created entity, just refresh
        await load();
        emit(state.copyWith(submitting: false));
      }
      return created;
    } catch (e) {
      emit(state.copyWith(submitting: false, submitError: e.toString()));
      return null;
    }
  }
}
