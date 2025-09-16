import 'package:micro_web/data/models/responses/user_response.dart';

import '../models/requests/admin_assign_role_for_verification_request.dart';
import '../models/requests/admin_create_user_request.dart';
import '../models/requests/create_role_request.dart';
import '../models/requests/create_verification_request.dart';
import '../models/requests/update_role_request.dart';
import '../models/requests/update_user_request.dart';
import '../models/users_verification_decision_request.dart';
import '../models/verification_decision_request.dart';
import '../models/responses/role_response.dart';
import '../services/api_client.dart';
import 'users_repository.dart';

/// Users service repository calling endpoints via the API Gateway base URL.
class RestUsersRepository implements UsersRepository {
  RestUsersRepository(this._api);
  final ApiClient _api;

  @override
  Future<UserResponse> getMe() async {
    final data = await _api.get('/me');
    return UserResponse.fromJson(data);
  }

  @override
  Future<List<UserResponse>> listUsers({Map<String, dynamic>? query}) async {
    final data = await _api.get('/users', query: query);
    return UserResponse.listFromJson(data);
  }

  @override
  Future<dynamic> getUserById(String id) async {
    return await _api.get('/users/$id');
  }

  @override
  Future<UserResponse> updateUser(String id, UpdateUserRequest request) async {
    final data = await _api.put('/users/$id', body: request.toJson());
    return UserResponse.fromJson(Map<String, dynamic>.from(data as Map));
  }

  @override
  Future<dynamic> adminCreateUser(AdminCreateUserRequest request) async {
    return await _api.post('/users', body: request.toJson());
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

  @override
  Future<List<RoleResponse>> listRoles({Map<String, dynamic>? query}) async {
    final data = await _api.get('/roles', query: query);
    return RoleResponse.listFromJson(data);
  }
}
