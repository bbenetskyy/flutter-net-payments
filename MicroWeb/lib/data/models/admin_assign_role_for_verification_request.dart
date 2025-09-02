//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

part 'admin_assign_role_for_verification_request.g.dart';


@JsonSerializable(
  checked: true,
  createToJson: true,
  disallowUnrecognizedKeys: false,
  explicitToJson: true,
)
class AdminAssignRoleForVerificationRequest {
  /// Returns a new [AdminAssignRoleForVerificationRequest] instance.
  AdminAssignRoleForVerificationRequest({

     this.desiredRoleId,
  });

  @JsonKey(
    
    name: r'desiredRoleId',
    required: false,
    includeIfNull: false,
  )


  final String? desiredRoleId;





    @override
    bool operator ==(Object other) => identical(this, other) || other is AdminAssignRoleForVerificationRequest &&
      other.desiredRoleId == desiredRoleId;

    @override
    int get hashCode =>
        (desiredRoleId == null ? 0 : desiredRoleId.hashCode);

  factory AdminAssignRoleForVerificationRequest.fromJson(Map<String, dynamic> json) => _$AdminAssignRoleForVerificationRequestFromJson(json);

  Map<String, dynamic> toJson() => _$AdminAssignRoleForVerificationRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }

}

