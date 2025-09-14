import '../models/requests/admin_assign_role_for_verification_request.dart';
import '../models/requests/admin_create_user_request.dart';
import '../models/requests/create_role_request.dart';
import '../models/requests/create_verification_request.dart';
import '../models/requests/update_role_request.dart';
import '../models/requests/update_user_request.dart';
import '../models/responses/user_response.dart';
import '../models/responses/role_response.dart';
import '../models/users_verification_decision_request.dart';
import '../models/verification_decision_request.dart';

/// Abstraction for Users service REST calls.
/// All calls should be routed through the API Gateway base URL (e.g., http://localhost:5264)
/// even if the swagger is hosted elsewhere.
abstract class UsersRepository {
  // Basic users
  Future<UserResponse> getMe();
  Future<List<UserResponse>> listUsers({Map<String, dynamic>? query});
  Future<dynamic> getUserById(String id);
  Future<dynamic> updateUser(String id, UpdateUserRequest request);

  // Admin users
  Future<dynamic> adminCreateUser(AdminCreateUserRequest request);
  Future<dynamic> adminUpdateUser(String id, UpdateUserRequest request);

  // Roles
  Future<dynamic> createRole(CreateRoleRequest request);
  Future<dynamic> updateRole(String id, UpdateRoleRequest request);
  // List roles available in the system
  Future<List<RoleResponse>> listRoles({Map<String, dynamic>? query});

  // Verification & role assignment flows
  Future<void> adminAssignRoleForVerification(String userId, AdminAssignRoleForVerificationRequest request);
  Future<dynamic> createVerification(CreateVerificationRequest request);
  Future<void> usersVerificationDecision(String userId, UsersVerificationDecisionRequest request);
  Future<void> verificationDecision(String verificationId, VerificationDecisionRequest request);
}
