//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

part 'admin_create_user_request.g.dart';


@JsonSerializable(
  checked: true,
  createToJson: true,
  disallowUnrecognizedKeys: false,
  explicitToJson: true,
)
class AdminCreateUserRequest {
  /// Returns a new [AdminCreateUserRequest] instance.
  AdminCreateUserRequest({

     this.email,

     this.displayName,

     this.desiredRoleId,

     this.dateOfBirth,
  });

  @JsonKey(
    
    name: r'email',
    required: false,
    includeIfNull: false,
  )


  final String? email;



  @JsonKey(
    
    name: r'displayName',
    required: false,
    includeIfNull: false,
  )


  final String? displayName;



  @JsonKey(
    
    name: r'desiredRoleId',
    required: false,
    includeIfNull: false,
  )


  final String? desiredRoleId;



  @JsonKey(
    
    name: r'dateOfBirth',
    required: false,
    includeIfNull: false,
  )


  final DateTime? dateOfBirth;





    @override
    bool operator ==(Object other) => identical(this, other) || other is AdminCreateUserRequest &&
      other.email == email &&
      other.displayName == displayName &&
      other.desiredRoleId == desiredRoleId &&
      other.dateOfBirth == dateOfBirth;

    @override
    int get hashCode =>
        (email == null ? 0 : email.hashCode) +
        (displayName == null ? 0 : displayName.hashCode) +
        (desiredRoleId == null ? 0 : desiredRoleId.hashCode) +
        (dateOfBirth == null ? 0 : dateOfBirth.hashCode);

  factory AdminCreateUserRequest.fromJson(Map<String, dynamic> json) => _$AdminCreateUserRequestFromJson(json);

  Map<String, dynamic> toJson() => _$AdminCreateUserRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }

}

