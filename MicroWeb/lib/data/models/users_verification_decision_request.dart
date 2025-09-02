//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

part 'users_verification_decision_request.g.dart';


@JsonSerializable(
  checked: true,
  createToJson: true,
  disallowUnrecognizedKeys: false,
  explicitToJson: true,
)
class UsersVerificationDecisionRequest {
  /// Returns a new [UsersVerificationDecisionRequest] instance.
  UsersVerificationDecisionRequest({

     this.targetId,

     this.code,

     this.accept,

     this.newPassword,
  });

  @JsonKey(
    
    name: r'targetId',
    required: false,
    includeIfNull: false,
  )


  final String? targetId;



  @JsonKey(
    
    name: r'code',
    required: false,
    includeIfNull: false,
  )


  final String? code;



  @JsonKey(
    
    name: r'accept',
    required: false,
    includeIfNull: false,
  )


  final bool? accept;



  @JsonKey(
    
    name: r'newPassword',
    required: false,
    includeIfNull: false,
  )


  final String? newPassword;





    @override
    bool operator ==(Object other) => identical(this, other) || other is UsersVerificationDecisionRequest &&
      other.targetId == targetId &&
      other.code == code &&
      other.accept == accept &&
      other.newPassword == newPassword;

    @override
    int get hashCode =>
        targetId.hashCode +
        (code == null ? 0 : code.hashCode) +
        accept.hashCode +
        (newPassword == null ? 0 : newPassword.hashCode);

  factory UsersVerificationDecisionRequest.fromJson(Map<String, dynamic> json) => _$UsersVerificationDecisionRequestFromJson(json);

  Map<String, dynamic> toJson() => _$UsersVerificationDecisionRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }

}

