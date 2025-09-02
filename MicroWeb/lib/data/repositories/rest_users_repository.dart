import '../models/admin_assign_role_for_verification_request.dart';
import '../models/admin_create_user_request.dart';
import '../models/create_role_request.dart';
import '../models/create_verification_request.dart';
import '../models/update_role_request.dart';
import '../models/update_user_request.dart';
import '../models/users_verification_decision_request.dart';
import '../models/verification_decision_request.dart';
import '../services/api_client.dart';
import 'users_repository.dart';

/// Users service repository calling endpoints via the API Gateway base URL.
class RestUsersRepository implements UsersRepository {
  RestUsersRepository(this._api);
  final ApiClient _api;

  @override
  Future<dynamic> getMe() async {
    final data = await _api.get('/users/me');
    return data;
    // If your gateway exposes /auth/me instead, switch to: return await _api.get('/auth/me');
  }

  @override
  Future<dynamic> listUsers({Map<String, dynamic>? query}) async {
    return await _api.get('/users', query: query);
  }

  @override
  Future<dynamic> getUserById(String id) async {
    return await _api.get('/users/$id');
  }

  @override
  Future<dynamic> updateUser(String id, UpdateUserRequest request) async {
    return await _api.put('/users/$id', body: request.toJson());
  }

  @override
  Future<dynamic> adminCreateUser(AdminCreateUserRequest request) async {
    return await _api.post('/admin/users', body: request.toJson());
  }

  @override
  Future<dynamic> adminUpdateUser(String id, UpdateUserRequest request) async {
    return await _api.put('/admin/users/$id', body: request.toJson());
  }

  @override
  Future<dynamic> createRole(CreateRoleRequest request) async {
    return await _api.post('/roles', body: request.toJson());
  }

  @override
  Future<dynamic> updateRole(String id, UpdateRoleRequest request) async {
    return await _api.put('/roles/$id', body: request.toJson());
  }

  @override
  Future<void> adminAssignRoleForVerification(String userId, AdminAssignRoleForVerificationRequest request) async {
    await _api.post('/admin/users/$userId/roles/assign-for-verification', body: request.toJson());
  }

  @override
  Future<dynamic> createVerification(CreateVerificationRequest request) async {
    return await _api.post('/verifications', body: request.toJson());
  }

  @override
  Future<void> usersVerificationDecision(String userId, UsersVerificationDecisionRequest request) async {
    await _api.post('/users/$userId/verification/decision', body: request.toJson());
  }

  @override
  Future<void> verificationDecision(String verificationId, VerificationDecisionRequest request) async {
    await _api.post('/verifications/$verificationId/decision', body: request.toJson());
  }
}
