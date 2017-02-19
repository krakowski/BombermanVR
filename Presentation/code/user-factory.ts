class UserFactory{

  public static createUser(id: number, fullName: FullName): User{
    return new User(id, fullName);
  }
  
}
